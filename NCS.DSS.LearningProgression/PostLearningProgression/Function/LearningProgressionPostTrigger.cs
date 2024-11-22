using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.Constants;
using NCS.DSS.LearningProgression.Cosmos.Helper;
using NCS.DSS.LearningProgression.PostLearningProgression.Service;
using NCS.DSS.LearningProgression.Validators;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.LearningProgression.PostLearningProgression.Function
{
    public class LearningProgressionPostTrigger
    {
        private const string RouteValue = "customers/{customerId}/LearningProgressions";
        private const string FunctionName = "POST";

        private readonly ILearningProgressionPostTriggerService _learningProgressionPostTriggerService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ILogger<LearningProgressionPostTrigger> _logger;
        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public LearningProgressionPostTrigger(ILearningProgressionPostTriggerService learningProgressionPostTriggerService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            IValidate validate,
            IDynamicHelper dynamicHelper,
            ILogger<LearningProgressionPostTrigger> logger)
        {
            _learningProgressionPostTriggerService = learningProgressionPostTriggerService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
            _logger = logger;
        }

        [Function(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Learning progression created.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this learning progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "Learning progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.LearningProgression), (int)HttpStatusCode.OK)]
        [Display(Name = "POST", Description = "Ability to create a new learning progression for a customer. <br>" +
                                              "<br> <b>Validation Rules:</b> <br>" +
                                              "<br><b>LearningHours:</b> A valid LearningHours contained in the enum. If CurrentLearningStatus = 'in learning' then this must be a valid LearningHours reference data item<br>" +
                                              "<br><b>DateLearningStarted:</b> If CurrentLearningStatus = 'In learning' then this must be a valid date, ISO8601:2004 <= datetime.now  <br>" +
                                              "<br><b>DateQualificationLevelAchieved:</b> If CurrentQualificationLevel < 99 then this must be a valid date, ISO8601:2004 <= datetime.now <br>"
                                                  )]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodPost, Route = RouteValue)] HttpRequest req, string customerId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(LearningProgressionPostTrigger));

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'TouchpointId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestResult();
            }

            var apimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimURL))
            {
                _logger.LogInformation("Unable to locate 'apimURL' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogInformation("Unable to parse 'customerId' to a GUID. Customer ID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerId, correlationGuid);
                return new BadRequestObjectResult(customerGuid);
            }

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}. Correlation GUID: {CorrelationGuid}", touchpointId, correlationGuid);
            
            _logger.LogInformation("Attempting to check if customer exists. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _logger.LogInformation("Customer does not exist. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new BadRequestResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to check if customer is read only. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogError("Customer is read-only. Operation is forbidden. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }
            _logger.LogInformation("Customer is not read-only. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            
            _logger.LogInformation("Attempting to check if LearningProgression exists for customer. Customer GUID: {CustomerId}", customerGuid);
            var doesLearningProgressionExist = _learningProgressionPostTriggerService.DoesLearningProgressionExistForCustomer(customerGuid);
            if (doesLearningProgressionExist)
            {
                _logger.LogInformation("LearningProgression for customer already exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new ConflictResult();
            }

            Models.LearningProgression learningProgression;

            try
            {
                _logger.LogInformation("Attempting to get resource from request body. Correlation GUID: {CorrelationGuid}", correlationGuid);
                learningProgression = await _httpRequestHelper.GetResourceFromRequest<Models.LearningProgression>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to parse LearningProgression from request body. Correlation GUID: {CorrelationGuid}. Exception: {ExceptionMessage}", correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
            }

            _learningProgressionPostTriggerService.SetIds(learningProgression, customerGuid, touchpointId);

            _logger.LogInformation("Attempting to validate {LearningProgression} object. Correlation GUID: {CorrelationGuid}", nameof(learningProgression), correlationGuid);
            var errors = _validate.ValidateResource(learningProgression);

            if (errors.Any())
            {
                _logger.LogError("Validation for {LearningProgression} object has failed. Correlation GUID: {CorrelationGuid}", nameof(learningProgression), correlationGuid);
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Validation for {LearningProgression} object has passed. Correlation GUID: {CorrelationGuid}", nameof(learningProgression), correlationGuid);

            _logger.LogInformation("Attempting to create LearningProgression object. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var learningProgressionResult = await _learningProgressionPostTriggerService.CreateLearningProgressionAsync(learningProgression);
            if (learningProgressionResult == null)
            {
                _logger.LogInformation("Failed to create LearningProgression object. Customer GUID: {CustomerGuid}.  Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new BadRequestObjectResult(customerGuid);
            }

            _logger.LogInformation("Successfully created LearningProgression object. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            
            _logger.LogInformation("Sending newly created LearningProgression to service bus. Customer GUID: {CustomerGuid}. Learning Progression ID: {LearningProgressionId}. Correlation GUID: {CorrelationGuid}", customerGuid, learningProgressionResult.LearningProgressionId.GetValueOrDefault(), correlationGuid);
            await _learningProgressionPostTriggerService.SendToServiceBusQueueAsync(learningProgression, apimURL, correlationGuid);

            if (learningProgression == null)
            {
                _logger.LogError("POST request unsuccessful. Learning Progression ID: {LearningProgressionGuid}", learningProgressionResult.LearningProgressionId.GetValueOrDefault());
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(LearningProgressionPostTrigger));
                return new NoContentResult();
            }

            _logger.LogInformation("POST request successful. Learning Progression ID: {LearningProgressionId}", learningProgression.LearningProgressionId.GetValueOrDefault());
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(LearningProgressionPostTrigger));
            return new JsonResult(learningProgression, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}