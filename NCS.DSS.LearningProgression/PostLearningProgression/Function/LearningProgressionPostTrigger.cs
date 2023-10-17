using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
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
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression
{
    public class LearningProgressionPostTrigger
    {
        const string RouteValue = "customers/{customerId}/LearningProgressions";
        const string FunctionName = "Post";
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILearningProgressionPostTriggerService _learningProgressionPostTriggerService;
        private readonly IJsonHelper _jsonHelper;
        private readonly IResourceHelper _resourceHelper;      
        private readonly ILoggerHelper _loggerHelper;        
        private Models.LearningProgression learningProgression;
        private readonly IValidate _validate;

        public LearningProgressionPostTrigger(
            
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IHttpRequestHelper httpRequestHelper,
            ILearningProgressionPostTriggerService learningProgressionPostTriggerService,
            IJsonHelper jsonHelper,
            IResourceHelper resourceHelper,
            IValidate validate,
            ILoggerHelper loggerHelper
            )
        {
            
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _httpRequestHelper = httpRequestHelper;
            _learningProgressionPostTriggerService = learningProgressionPostTriggerService;
            _jsonHelper = jsonHelper;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _loggerHelper = loggerHelper;
        }

        [FunctionName(FunctionName)]
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
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodPost, Route = RouteValue)]HttpRequest req, ILogger logger, string customerId)
        {
            _loggerHelper.LogMethodEnter(logger);

            logger.LogInformation($"Started Updating Learning Progressions for Customer ID [{customerId}]");


            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            var guidHelper = new GuidHelper();
            var correlationGuid = guidHelper.ValidateAndGetGuid(correlationId);

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, "Unable to locate 'TouchpointId' in request header.");
                return _httpResponseMessageHelper.BadRequest();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, "Unable to locate 'apimurl' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"Unable to parse 'customerId' to a Guid: {customerId}");
                return _httpResponseMessageHelper.BadRequest(customerGuid);
            }

            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, "Bad request");
                return _httpResponseMessageHelper.BadRequest();
            }

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"Customer is readonly with customerId {customerGuid}.");
                return _httpResponseMessageHelper.Forbidden(customerGuid);
            }

            var doesLearningProgressionExist = _learningProgressionPostTriggerService.DoesLearningProgressionExistForCustomer(customerGuid);
            if (doesLearningProgressionExist)
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"Learning progression details already exists for customerId {customerGuid}.");
                return _httpResponseMessageHelper.Conflict();
            }

            _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Attempt to get resource from body of the request correlationId {correlationGuid}.");

            try
            {
                learningProgression = await _httpRequestHelper.GetResourceFromRequest<Models.LearningProgression>(req);
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(logger, correlationGuid, "Unable to retrieve body from req", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(JObject.FromObject(new { Error = ex.Message }).ToString());
            }

            _learningProgressionPostTriggerService.SetIds(learningProgression, customerGuid, touchpointId);

            var errors = _validate.ValidateResource(learningProgression);

            if (errors.Any())
            {
                logger.LogError($"validation errors with resource correlationId {correlationGuid}. List of Errors found [{string.Join(';',errors)}]");
                
                return _httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            var learningProgressionResult = await _learningProgressionPostTriggerService.CreateLearningProgressionAsync(learningProgression);
            if (learningProgressionResult == null)
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"Unable to create learning progression for customerId {customerGuid}.");
                return _httpResponseMessageHelper.BadRequest(customerGuid);
            }

            _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Sending newly created learning Progression to service bus for customerId {customerGuid}, correlationId {correlationGuid}.");
            await _learningProgressionPostTriggerService.SendToServiceBusQueueAsync(learningProgression, ApimURL, correlationGuid, logger);

            _loggerHelper.LogMethodExit(logger);

            if (learningProgression == null)
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"Response Code [No Content]");
                return _httpResponseMessageHelper.NoContent(customerGuid);
            }

            _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Response Code [Created]");

            return _httpResponseMessageHelper.Created(_jsonHelper.SerializeObjectAndRenameIdProperty(learningProgression, "id", "LearningProgressionId"));
        }
    }
}