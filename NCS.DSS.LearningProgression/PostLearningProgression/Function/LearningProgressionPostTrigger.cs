using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.Constants;
using NCS.DSS.LearningProgression.PostLearningProgression.Service;
using NCS.DSS.LearningProgression.Validators;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using NCS.DSS.LearningProgression.Cosmos.Helper;

namespace NCS.DSS.LearningProgression.PostLearningProgression.Function
{
    public class LearningProgressionPostTrigger
    {
        private const string RouteValue = "customers/{customerId}/LearningProgressions";
        private const string FunctionName = "Post";
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILearningProgressionPostTriggerService _learningProgressionPostTriggerService;
        private readonly IResourceHelper _resourceHelper;      
        private readonly ILogger<LearningProgressionPostTrigger> _logger;
        private Models.LearningProgression _learningProgression;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private static readonly string[] PropertyToExclude = {"TargetSite"};

        public LearningProgressionPostTrigger(
            
            IHttpRequestHelper httpRequestHelper,
            ILearningProgressionPostTriggerService learningProgressionPostTriggerService,
            IResourceHelper resourceHelper,
            IValidate validate,
            ILogger<LearningProgressionPostTrigger> logger,
            IDynamicHelper dynamicHelper
            )
        {
            _httpRequestHelper = httpRequestHelper;
            _learningProgressionPostTriggerService = learningProgressionPostTriggerService;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _logger = logger;
            _dynamicHelper = dynamicHelper;
        }

        [Function(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Learning progression created.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this learning progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Learning progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.LearningProgression), (int)HttpStatusCode.OK)]
        [Display(Name = "Post", Description = "Ability to create a new learning progression for a customer. <br>" +
                                              "<br> <b>Validation Rules:</b> <br>" +
                                              "<br><b>LearningHours:</b> A valid LearningHours contained in the enum. If CurrentLearningStatus = 'in learning' then this must be a valid LearningHours reference data item<br>" +
                                              "<br><b>DateLearningStarted:</b> If CurrentLearningStatus = 'In learning' then this must be a valid date, ISO8601:2004 <= datetime.now  <br>" +
                                              "<br><b>DateQualificationLevelAchieved:</b> If CurrentQualificationLevel < 99 then this must be a valid date, ISO8601:2004 <= datetime.now <br>"
                                                  )]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodPost, Route = RouteValue)]HttpRequest req, string customerId)
        {
            _logger.LogInformation("Started Updating Learning Progressions for Customer ID [{0}]", customerId);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            var guidHelper = new GuidHelper();
            var correlationGuid = guidHelper.ValidateAndGetGuid(correlationId);

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("CorrelationId: {0} Unable to locate 'TouchpointId' in request header.", correlationGuid);
                return new BadRequestResult();
            }

            var apimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimURL))
            {
                _logger.LogWarning("CorrelationId: {0} Unable to locate 'apimurl' in request header", correlationGuid);
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("CorrelationId: {0} Unable to parse 'customerId' to a Guid: {1}", correlationGuid, customerId);
                return new BadRequestObjectResult(customerGuid);
            }

            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _logger.LogWarning("CorrelationId: {0} Bad request", correlationGuid);
                return new BadRequestResult();
            }

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogWarning("CorrelationId: {0} Customer is readonly with customerId: {1}", correlationGuid, customerId);
                return new ForbidResult(customerGuid.ToString());
            }

            var doesLearningProgressionExist = _learningProgressionPostTriggerService.DoesLearningProgressionExistForCustomer(customerGuid);
            if (doesLearningProgressionExist)
            {
                _logger.LogWarning("CorrelationId: {0} Learning progression details already exists for customerId: {1}", correlationGuid, customerId);
                return new ConflictResult();
            }

            _logger.LogInformation("CorrelationId: {0} Attempt to get resource from body of the request correlationId: {1}", correlationGuid, correlationGuid);

            try
            {
                _learningProgression = await _httpRequestHelper.GetResourceFromRequest<Models.LearningProgression>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError("CorrelationId: {0} Unable to retrieve body from req - {1}", correlationGuid, ex);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
            }

            _learningProgressionPostTriggerService.SetIds(_learningProgression, customerGuid, touchpointId);

            var errors = _validate.ValidateResource(_learningProgression);

            if (errors.Any())
            {
                _logger.LogWarning("CorrelationId: {0} Validation errors. List of Errors [{2}]", correlationGuid, string.Join(';',errors));
                return new UnprocessableEntityObjectResult(errors);
            }

            var learningProgressionResult = await _learningProgressionPostTriggerService.CreateLearningProgressionAsync(_learningProgression);
            if (learningProgressionResult == null)
            {
                _logger.LogWarning("CorrelationId: {0} Unable to create learning progression for customerId {1}", correlationGuid, customerGuid);
                return new BadRequestObjectResult(customerGuid);
            }

            _logger.LogWarning("CorrelationId: {0} Sending newly created learning Progression to service bus for customerId: {1}", correlationGuid, customerGuid);
            await _learningProgressionPostTriggerService.SendToServiceBusQueueAsync(_learningProgression, apimURL, correlationGuid, _logger);

            if (_learningProgression == null)
            {
                _logger.LogWarning("CorrelationId: {0} Response Code [No Content]", correlationGuid);
                return new NoContentResult();
            }

            _logger.LogInformation("CorrelationId: {0} Response Code [Created]", correlationGuid);

            return new JsonResult(_learningProgression, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}