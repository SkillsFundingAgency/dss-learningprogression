using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.LearningProgression.Constants;
using System.Net;
using DFC.Swagger.Standard.Annotations;
using System.Net.Http;
using System;
using DFC.HTTP.Standard;
using Newtonsoft.Json;
using DFC.Common.Standard.GuidHelper;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.CosmosDocumentClient;
using NCS.DSS.LearningProgression.Validators;
using NCS.DSS.LearningProgression.Models;
using System.Linq;
using DFC.Common.Standard.Logging;
using NCS.DSS.LearningProgression.PatchLearningProgression.Service;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.LearningProgression.Tests.FunctionTests
{
    public class LearningProgressionPatchTrigger
    {
        const string RouteValue = "customers/{customerId}/learningprogessions/{LearningProgessionId}";
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
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodPatch, Route = RouteValue)]HttpRequest req, ILogger logger, string customerId, string LearningProgessionId)
        {
            _loggerHelper.LogMethodEnter(logger);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            var guidHelper = new GuidHelper();
            var correlationGuid = guidHelper.ValidateGuid(correlationId);

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Unable to locate 'TouchpointId' in request header.");

                return _httpResponseMessageHelper.BadRequest();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Unable to locate 'apimurl' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Unable to parse 'customerId' to a Guid: {customerId}");
                return _httpResponseMessageHelper.BadRequest(customerGuid);
            }

            if (!Guid.TryParse(LearningProgessionId, out var learnerProgressionGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Unable to parse 'learnerProgressionId' to a Guid: {learnerProgressionGuid}");
                return _httpResponseMessageHelper.BadRequest(learnerProgressionGuid);
            }

            LearningProgressionPatch learningProgressionPatchRequest;
            learningProgressionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<LearningProgressionPatch>(req);

            if (learningProgressionPatchRequest == null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"A patch body was not provided. CorrelationId: {correlationGuid}.");
                return _httpResponseMessageHelper.NoContent();
            }

            _learningProgressionPatchTriggerService.SetIds(learningProgressionPatchRequest, customerGuid, touchpointId);
            if (await _resourceHelper.IsCustomerReadOnly(customerGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Customer is readonly with customerId {customerGuid}.");
                return _httpResponseMessageHelper.Forbidden(customerGuid);
            }

            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Bad request");
                return _httpResponseMessageHelper.BadRequest();
            }

            if (!_learningProgressionPatchTriggerService.DoesLearningProgressionExistForCustomer(customerGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Learning progression does not exist for customerId {customerGuid}.");
                return _httpResponseMessageHelper.NoContent();
            }

            var currentLearningProgressionAsJson = await _learningProgressionPatchTriggerService.GetLearningProgressionForCustomerToPatchAsync(customerGuid, learnerProgressionGuid);

            if (currentLearningProgressionAsJson == null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Learning progression does not exist for {learnerProgressionGuid}.");
                return _httpResponseMessageHelper.NoContent(learnerProgressionGuid);
            }

            var patchedLearningProgressionAsJson = _learningProgressionPatchTriggerService.PatchLearningProgressionAsync(currentLearningProgressionAsJson, learningProgressionPatchRequest);
            if (patchedLearningProgressionAsJson == null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Learning progression does not exist for {learnerProgressionGuid}.");
                return _httpResponseMessageHelper.NoContent(learnerProgressionGuid);
            }

            Models.LearningProgression learningProgressionValidationObject;
            try
            {
                learningProgressionValidationObject = JsonConvert.DeserializeObject<Models.LearningProgression>(patchedLearningProgressionAsJson);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogError(logger, correlationGuid, "Unable to retrieve body from req", ex);
                _loggerHelper.LogError(logger, correlationGuid, ex);
                throw;
            }

            if (learningProgressionValidationObject == null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Learning Progression Validation Object is null.");
                return _httpResponseMessageHelper.UnprocessableEntity(req);
            }

            var errors = _validate.ValidateResource(learningProgressionValidationObject);
            if (errors != null && errors.Any())
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"validation errors with resource customerId {customerGuid}.");
                return _httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            var updatedLearningProgression = await _learningProgressionPatchTriggerService.UpdateCosmosAsync(patchedLearningProgressionAsJson, learnerProgressionGuid);
            if (updatedLearningProgression != null)
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"attempting to send to service bus {learnerProgressionGuid}.");

                await _learningProgressionPatchTriggerService.SendToServiceBusQueueAsync(updatedLearningProgression, ApimURL);
            }

            _loggerHelper.LogMethodExit(logger);

            return learningProgressionPatchRequest == null ?
            _httpResponseMessageHelper.NoContent(customerGuid) :
            _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(learningProgressionPatchRequest, "id", "LearningProgressionId"));
        }
    }
}