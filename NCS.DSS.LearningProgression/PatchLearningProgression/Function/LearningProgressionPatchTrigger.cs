using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.Constants;
using NCS.DSS.LearningProgression.Cosmos.Helper;
using NCS.DSS.LearningProgression.Models;
using NCS.DSS.LearningProgression.PatchLearningProgression.Service;
using NCS.DSS.LearningProgression.Validators;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace NCS.DSS.LearningProgression.PatchLearningProgression.Function
{
    public class LearningProgressionPatchTrigger
    {
        private const string RouteValue = "customers/{customerId}/learningprogressions/{LearningProgressionId}";
        private const string FunctionName = "Patch";
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILearningProgressionPatchTriggerService _learningProgressionPatchTriggerService;
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly ILogger<LearningProgressionPatchTrigger> _logger;
        private readonly IDynamicHelper _dynamicHelper;
        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public LearningProgressionPatchTrigger(

            IHttpRequestHelper httpRequestHelper,
            ILearningProgressionPatchTriggerService learningProgressionPatchTriggerService,
            IResourceHelper resourceHelper,
            IValidate validate,
            ILogger<LearningProgressionPatchTrigger> logger,
            IDynamicHelper dynamicHelper)
        {

            _httpRequestHelper = httpRequestHelper;
            _learningProgressionPatchTriggerService = learningProgressionPatchTriggerService;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _logger = logger;
            _dynamicHelper = dynamicHelper;
        }

        [Function(FunctionName)]
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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodPatch, Route = RouteValue)] HttpRequest req, string customerId, string LearningProgressionId)
        {
            _logger.LogInformation("Patching Learning Progression of ID [{0}] for Customer ID [{1}]", LearningProgressionId, customerId);


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

            if (!Guid.TryParse(LearningProgressionId, out var LearnerProgressionGuid))
            {
                _logger.LogWarning("CorrelationId: {0} Unable to parse 'learnerProgressionId' to a Guid: {1}", correlationGuid, LearnerProgressionGuid);
                return new BadRequestObjectResult(LearnerProgressionGuid);
            }

            LearningProgressionPatch learningProgressionPatchRequest;
            try
            {
                learningProgressionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<LearningProgressionPatch>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError("CorrelationId: {0} Unable to retrieve body from req - {1}", correlationGuid, ex);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
            }

            if (learningProgressionPatchRequest == null)
            {
                _logger.LogWarning("CorrelationId: {0} A patch body was not provided", correlationGuid);
                return new NoContentResult();
            }

            _learningProgressionPatchTriggerService.SetIds(learningProgressionPatchRequest, customerGuid, touchpointId);
            if (await _resourceHelper.IsCustomerReadOnly(customerGuid))
            {
                _logger.LogWarning("CorrelationId: {0} Customer is readonly with customerId {1}", correlationGuid, customerId);
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _logger.LogWarning("CorrelationId: {0} Bad request", correlationGuid);
                return new BadRequestResult();
            }

            if (!_learningProgressionPatchTriggerService.DoesLearningProgressionExistForCustomer(customerGuid))
            {
                _logger.LogWarning("CorrelationId: {0} Learning progression does not exist for customerId {1}", correlationGuid, customerId);
                return new NoContentResult();
            }

            var currentLearningProgressionAsJson = await _learningProgressionPatchTriggerService.GetLearningProgressionForCustomerToPatchAsync(customerGuid, LearnerProgressionGuid);

            if (currentLearningProgressionAsJson == null)
            {
                _logger.LogWarning("CorrelationId: {0} Learning progression does not exist for {1}", correlationGuid, LearnerProgressionGuid);
                return new NoContentResult();
            }

            var patchedLearningProgressionAsJson = _learningProgressionPatchTriggerService.PatchLearningProgressionAsync(currentLearningProgressionAsJson, learningProgressionPatchRequest);
            if (patchedLearningProgressionAsJson == null)
            {
                _logger.LogWarning("CorrelationId: {0} Learning progression does not exist for {1}", correlationGuid, LearnerProgressionGuid);
                return new NoContentResult();
            }

            LearningProgressionPatch learningProgressionValidationObject;
            try
            {
                learningProgressionValidationObject = JsonConvert.DeserializeObject<Models.LearningProgressionPatch>(patchedLearningProgressionAsJson);
            }
            catch (JsonException ex)
            {
                _logger.LogError("CorrelationId: {0} Unable to retrieve body from req - {exception}", correlationGuid, ex);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
            }

            if (learningProgressionValidationObject == null)
            {
                _logger.LogWarning("CorrelationId: {0} Learning Progression Validation Object is null.", correlationGuid);
                return new UnprocessableEntityObjectResult(req);
            }

            learningProgressionValidationObject.LastModifiedTouchpointId = touchpointId;

            var errors = _validate.ValidateResource(learningProgressionValidationObject);
            if (errors != null && errors.Any())
            {
                _logger.LogWarning("CorrelationId: {0} Validation errors with resource customerId {1}. List of Errors [{2}]", correlationGuid, customerGuid, string.Join(';', errors));
                return new UnprocessableEntityObjectResult(errors);
            }

            var updatedLearningProgression = await _learningProgressionPatchTriggerService.UpdateCosmosAsync(patchedLearningProgressionAsJson, LearnerProgressionGuid);
            if (updatedLearningProgression != null)
            {
                _logger.LogInformation("CorrelationId: {0} Attempting to send to service bus {1}", correlationGuid, LearnerProgressionGuid);

                await _learningProgressionPatchTriggerService.SendToServiceBusQueueAsync(updatedLearningProgression, customerGuid, apimURL, correlationGuid, _logger);

                _logger.LogInformation("CorrelationId: {0} Ok", correlationGuid);

                return new JsonResult(updatedLearningProgression, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            _logger.LogWarning("CorrelationId: {0} No Content", correlationGuid);
            return new NoContentResult();
        }
    }
}