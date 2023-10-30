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
using NCS.DSS.LearningProgression.Models;
using NCS.DSS.LearningProgression.PatchLearningProgression.Service;
using NCS.DSS.LearningProgression.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.Tests.FunctionTests
{
    public class LearningProgressionPatchTrigger
    {
        const string RouteValue = "customers/{customerId}/learningprogressions/{LearningProgressionId}";
        const string FunctionName = "Patch";
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILearningProgressionPatchTriggerService _learningProgressionPatchTriggerService;
        private readonly IJsonHelper _jsonHelper;
        private readonly IResourceHelper _resourceHelper;  
        private readonly IValidate _validate;
        private readonly ILoggerHelper _loggerHelper;

        public LearningProgressionPatchTrigger(
            
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IHttpRequestHelper httpRequestHelper,
            ILearningProgressionPatchTriggerService learningProgressionPatchTriggerService,
            IJsonHelper jsonHelper,
            IResourceHelper resourceHelper,
            IValidate validate,
            ILoggerHelper loggerHelper
            )
        {
            
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _httpRequestHelper = httpRequestHelper;
            _learningProgressionPatchTriggerService = learningProgressionPatchTriggerService;
            _jsonHelper = jsonHelper;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _loggerHelper = loggerHelper;
        }

        [FunctionName(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Learning progression created.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer Resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this learning progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Learning progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.LearningProgression), (int)HttpStatusCode.OK)]
        [Display(Name = "Patch", Description = "Ability to modify/update learning progression for a customer. <br>" +
                                              "<br> <b>Validation Rules:</b> <br>" +
                                              "<br><b>LearningHours:</b> A valid LearningHours contained in the enum. If CurrentLearningStatus = 'in learning' then this must be a valid LearningHours reference data item<br>" +
                                              "<br><b>DateLearningStarted:</b> If CurrentLearningStatus = 'In learning' then this must be a valid date, ISO8601:2004 <= datetime.now  <br>" +
                                              "<br><b>DateQualificationLevelAchieved:</b> If CurrentQualificationLevel < 99 then this must be a valid date, ISO8601:2004 <= datetime.now <br>"
                                                )]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodPatch, Route = RouteValue)]HttpRequest req, ILogger logger, string customerId, string LearningProgressionId)
        {
            _loggerHelper.LogMethodEnter(logger);

            logger.LogInformation($"Patching Learning Progression record with ID [{LearningProgressionId}] for Customer ID [{customerId}] ");

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

            if (!Guid.TryParse(LearningProgressionId, out var learnerProgressionGuid))
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"Unable to parse 'learnerProgressionId' to a Guid: {learnerProgressionGuid}");
                return _httpResponseMessageHelper.BadRequest(learnerProgressionGuid);
            }

            LearningProgressionPatch learningProgressionPatchRequest;
            try
            {
                learningProgressionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<LearningProgressionPatch>(req);
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(logger, correlationGuid, "Unable to retrieve body from req", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(JObject.FromObject(new { Error = ex.Message }).ToString());
            }            

            if (learningProgressionPatchRequest == null)
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"A patch body was not provided. CorrelationId: {correlationGuid}.");
                return _httpResponseMessageHelper.NoContent();
            }

            _learningProgressionPatchTriggerService.SetIds(learningProgressionPatchRequest, customerGuid, touchpointId);
            if (await _resourceHelper.IsCustomerReadOnly(customerGuid))
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"Customer is readonly with customerId {customerGuid}.");
                return _httpResponseMessageHelper.Forbidden(customerGuid);
            }

            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, "Bad request");
                return _httpResponseMessageHelper.BadRequest();
            }

            if (!_learningProgressionPatchTriggerService.DoesLearningProgressionExistForCustomer(customerGuid))
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"Learning progression does not exist for customerId {customerGuid}.");
                return _httpResponseMessageHelper.NoContent();
            }

            var currentLearningProgressionAsJson = await _learningProgressionPatchTriggerService.GetLearningProgressionForCustomerToPatchAsync(customerGuid, learnerProgressionGuid);

            if (currentLearningProgressionAsJson == null)
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"Learning progression does not exist for {learnerProgressionGuid}.");
                return _httpResponseMessageHelper.NoContent(learnerProgressionGuid);
            }

            var patchedLearningProgressionAsJson = _learningProgressionPatchTriggerService.PatchLearningProgressionAsync(currentLearningProgressionAsJson, learningProgressionPatchRequest);
            if (patchedLearningProgressionAsJson == null)
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"Learning progression does not exist for {learnerProgressionGuid}.");
                return _httpResponseMessageHelper.NoContent(learnerProgressionGuid);
            }

            Models.LearningProgressionPatch learningProgressionValidationObject;
            try
            {
                learningProgressionValidationObject = JsonConvert.DeserializeObject<Models.LearningProgressionPatch>(patchedLearningProgressionAsJson);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogException(logger, correlationGuid, "Unable to retrieve body from req", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(JObject.FromObject(new { Error = ex.Message }).ToString());
            }

            if (learningProgressionValidationObject == null)
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, "Learning Progression Validation Object is null.");
                return _httpResponseMessageHelper.UnprocessableEntity(req);
            }

            learningProgressionValidationObject.LastModifiedTouchpointId = touchpointId;

            var errors = _validate.ValidateResource(learningProgressionValidationObject);
            if (errors != null && errors.Any())
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"validation errors with resource customerId {customerGuid}. List of Errors [{string.Join(';',errors)}]");
                return _httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            var updatedLearningProgression = await _learningProgressionPatchTriggerService.UpdateCosmosAsync(patchedLearningProgressionAsJson, learnerProgressionGuid);
            if (updatedLearningProgression != null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"attempting to send to service bus {learnerProgressionGuid}.");

                await _learningProgressionPatchTriggerService.SendToServiceBusQueueAsync(updatedLearningProgression, customerGuid, ApimURL, correlationGuid, logger);

                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Ok");

                _loggerHelper.LogMethodExit(logger);

                return _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(updatedLearningProgression, "id", "LearningProgressionId"));
            }

            _loggerHelper.LogWarningMessage(logger, correlationGuid, "No Content");

            _loggerHelper.LogMethodExit(logger);

            return _httpResponseMessageHelper.NoContent(customerGuid);
           
        }
    }
}