using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.LearningProgression.Constants;
using NCS.DSS.LearningProgression.Cosmos.Helper;
using NCS.DSS.LearningProgression.GetLearningProgression.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.LearningProgression.GetLearningProgression.Function
{
    public class LearningProgressionsGetTrigger
    {
        private const string RouteValue = "customers/{customerId}/LearningProgressions";
        private const string FunctionName = "GET";
        private readonly ILearningProgressionsGetTriggerService _learningProgressionsGetTriggerService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<LearningProgressionsGetTrigger> _logger;

        public LearningProgressionsGetTrigger(ILearningProgressionsGetTriggerService learningProgressionsGetTriggerService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            ILogger<LearningProgressionsGetTrigger> logger)
        {
            _learningProgressionsGetTriggerService = learningProgressionsGetTriggerService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _logger = logger;
        }

        [Function(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Learning progression found.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this learning progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "Learning progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.LearningProgression), (int)HttpStatusCode.OK)]
        [Display(Name = "GET_BY_CUSTOMERID", Description = "Ability to return all learning progressions for the given customer.")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodGet, Route = RouteValue)]
            HttpRequest req, string customerId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(LearningProgressionsGetTrigger));

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
                _logger.LogWarning("Unable to locate 'TouchpointId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestResult();
            }

            var apimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimURL))
            {
                _logger.LogWarning("Unable to locate 'apimURL' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to parse 'customerId' to a GUID. Customer ID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerId, correlationGuid);
                return new BadRequestObjectResult(customerGuid);
            }

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}. Correlation GUID: {CorrelationGuid}", touchpointId, correlationGuid);

            _logger.LogInformation("Attempting to see if customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _logger.LogWarning("Customer does not exist. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new BadRequestResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to retrieve LearningProgressions for Customer. Customer GUID: {CustomerGuid}", customerGuid);
            var learningProgressions = await _learningProgressionsGetTriggerService.GetLearningProgressionsForCustomerAsync(customerGuid);
            
            if (learningProgressions == null)
            {
                _logger.LogInformation("LearningProgressions does not exist for Customer. Customer GUID: {CustomerGuid}", customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(LearningProgressionsGetTrigger));
                return new NoContentResult();
            }

            if (learningProgressions.Count == 1)
            {
                _logger.LogInformation("LearingProgressions successfully retrieved. Learning Progression ID: {LearningProgressionId}", learningProgressions.FirstOrDefault()!.Id);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(LearningProgressionsGetTrigger));
                return new JsonResult(learningProgressions[0], new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            var learningProgressionIds = learningProgressions.Select(lp => lp.Id).ToList();

            _logger.LogInformation("LearingProgressions successfully retrieved. Learning Progression IDs: {LearningProgressionIds}", learningProgressionIds);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(LearningProgressionsGetTrigger));
            return new JsonResult(learningProgressions, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
