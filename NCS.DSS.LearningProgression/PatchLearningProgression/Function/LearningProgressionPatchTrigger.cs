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
        private const string RouteValue = "customers/{customerId}/learningprogressions/{learningProgressionId}";
        private const string FunctionName = "PATCH";

        private readonly ILearningProgressionPatchTriggerService _learningProgressionPatchTriggerService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ILogger<LearningProgressionPatchTrigger> _logger;
        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public LearningProgressionPatchTrigger(ILearningProgressionPatchTriggerService learningProgressionPatchTriggerService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            IValidate validate,
            IDynamicHelper dynamicHelper,
            ILogger<LearningProgressionPatchTrigger> logger)
        {
            _learningProgressionPatchTriggerService = learningProgressionPatchTriggerService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
            _logger = logger;
        }

        [Function(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Learning progression created.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer Resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this learning progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "Learning progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.LearningProgression), (int)HttpStatusCode.OK)]
        [Display(Name = "PATCH", Description = "Ability to modify/update learning progression for a customer. <br>" +
                                              "<br> <b>Validation Rules:</b> <br>" +
                                              "<br><b>LearningHours:</b> A valid LearningHours contained in the enum. If CurrentLearningStatus = 'in learning' then this must be a valid LearningHours reference data item<br>" +
                                              "<br><b>DateLearningStarted:</b> If CurrentLearningStatus = 'In learning' then this must be a valid date, ISO8601:2004 <= datetime.now  <br>" +
                                              "<br><b>DateQualificationLevelAchieved:</b> If CurrentQualificationLevel < 99 then this must be a valid date, ISO8601:2004 <= datetime.now <br>"
                                                )]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodPatch, Route = RouteValue)] HttpRequest req,
            string customerId, string learningProgressionId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(LearningProgressionPatchTrigger));

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

            if (!Guid.TryParse(learningProgressionId, out var learningProgressionGuid))
            {
                _logger.LogInformation("Unable to parse 'learnerProgressionId' to a GUID. Customer ID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerId, correlationGuid);
                return new BadRequestObjectResult(learningProgressionGuid);
            }

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}. Correlation GUID: {CorrelationGuid}", touchpointId, correlationGuid);
            
            LearningProgressionPatch learningProgressionPatchRequest;
            try
            {
                _logger.LogInformation("Attempting to retrieve resource from request body. Correlation GUID: {CorrelationGuid}", correlationGuid);
                learningProgressionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<LearningProgressionPatch>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to parse {LearningProgressionPatch} from request body. Correlation GUID: {CorrelationGuid}. Exception: {ExceptionMessage}", nameof(learningProgressionPatchRequest), correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
            }
            _logger.LogInformation("Retrieved resource from request body. Correlation GUID: {CorrelationGuid}", correlationGuid);

            if (learningProgressionPatchRequest == null)
            {
                _logger.LogError("{LearningProgressionPatch} object is NULL. Correlation GUID: {CorrelationGuid}", nameof(learningProgressionPatchRequest), correlationGuid);
                return new NoContentResult();
            }

            _learningProgressionPatchTriggerService.SetIds(learningProgressionPatchRequest, customerGuid, touchpointId);

            _logger.LogInformation("Attempting to check if customer is read only. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            if (await _resourceHelper.IsCustomerReadOnly(customerGuid))
            {
                _logger.LogError("Customer is read-only. Operation is forbidden. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }
            _logger.LogInformation("Customer is not read-only. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to check if customer exists. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _logger.LogInformation("Customer does not exist. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new BadRequestResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to check if LearningProgression exists for customer. Customer GUID: {CustomerId}", customerGuid);
            if (!_learningProgressionPatchTriggerService.DoesLearningProgressionExistForCustomer(customerGuid))
            {
                _logger.LogInformation("LearningProgression does not exist for customer. Customer GUID: {CustomerGuid}", customerGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("LearningProgression for customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            var currentLearningProgressionAsJson = await _learningProgressionPatchTriggerService.GetLearningProgressionForCustomerToPatchAsync(customerGuid, learningProgressionGuid);

            if (currentLearningProgressionAsJson == null)
            {
                return new NoContentResult();
            }

            _logger.LogInformation("Attempting to update LearningProgression object for customer. Customer GUID: {CustomerGuid}. Learning Progression GUID: {LearningProgressionGuid}", customerGuid, learningProgressionGuid);
            var patchedLearningProgressionAsJson = _learningProgressionPatchTriggerService.PatchLearningProgressionAsync(currentLearningProgressionAsJson, learningProgressionPatchRequest);
            if (patchedLearningProgressionAsJson == null)
            {
                _logger.LogInformation("Failed to update LearningProgression for customer. Customer GUID: {CustomerGuid}. Learning Progression GUID: {LearningProgressionGuid}", customerGuid, learningProgressionGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Successfully updated LearningProgression object for customer. Customer GUID: {CustomerGuid}. Learning Progression GUID: {LearningProgressionGuid}", customerGuid, learningProgressionGuid);

            LearningProgressionPatch learningProgressionValidationObject;
            try
            {
                _logger.LogInformation("Attempting to deserialize {PatchedLearningProgressionAsJson} validation object. Customer GUID: {CustomerGuid}. Learning Progression GUID: {LearningProgressionGuid}", nameof(patchedLearningProgressionAsJson), customerGuid, learningProgressionGuid);
                learningProgressionValidationObject = JsonConvert.DeserializeObject<LearningProgressionPatch>(patchedLearningProgressionAsJson);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "An error occured when attempting to deserialize {PatchedLearningProgressionAsJson} validation object. Error message: {ErrorMessage}. Customer GUID: {CustomerGuid}. Learning Progression GUID: {LearningProgressionGuid}", nameof(patchedLearningProgressionAsJson), ex.Message, customerGuid, learningProgressionGuid);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
            }

            if (learningProgressionValidationObject == null)
            {
                _logger.LogInformation("Deserializing {PatchedLearningProgressionAsJson} validation object has returned NULL. Customer GUID: {CustomerGuid}. Learning Progression GUID: {LearningProgressionGuid}", nameof(patchedLearningProgressionAsJson), customerGuid, learningProgressionGuid);
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation("Successfully deserialized {PatchedLearningProgressionAsJson} validation object. Customer GUID: {CustomerGuid}. Learning Progression GUID: {LearningProgressionGuid}", nameof(patchedLearningProgressionAsJson), customerGuid, learningProgressionGuid);

            learningProgressionValidationObject.LastModifiedTouchpointId = touchpointId;

            _logger.LogInformation("Attempting to validate {LearningProgressionValidationObject} object", nameof(learningProgressionValidationObject));
            var errors = _validate.ValidateResource(learningProgressionValidationObject);
            
            if (errors != null && errors.Any())
            {
                _logger.LogError("Falied to validate {LearningProgressionValidationObject}", nameof(learningProgressionValidationObject));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Successfully validated {LearningProgressionValidationObject}", nameof(learningProgressionValidationObject));

            _logger.LogInformation("Attempting to PATCH a LearningProgression. Customer GUID: {CustomerGuid}", customerGuid);
            var updatedLearningProgression = await _learningProgressionPatchTriggerService.UpdateCosmosAsync(patchedLearningProgressionAsJson, learningProgressionGuid);
            
            if (updatedLearningProgression != null)
            {
                _logger.LogInformation("Sending newly created LearningProgression to service bus. Customer GUID: {CustomerGuid}. Learning Progression ID: {LearningProgressionId}. Correlation GUID: {CorrelationGuid}", customerGuid, updatedLearningProgression.LearningProgressionId.GetValueOrDefault(), correlationGuid);
                await _learningProgressionPatchTriggerService.SendToServiceBusQueueAsync(updatedLearningProgression, customerGuid, apimURL, correlationGuid);

                _logger.LogInformation("PATCH request successful. Learning Progression ID: {LearningProgressionId}", updatedLearningProgression.LearningProgressionId.GetValueOrDefault());
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(LearningProgressionPatchTrigger));
                return new JsonResult(updatedLearningProgression, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            _logger.LogError("PATCH request unsuccessful. Learning Progression GUID: {LearningProgressionGuid}", learningProgressionGuid);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(LearningProgressionPatchTrigger));
            return new NoContentResult();
        }
    }
}