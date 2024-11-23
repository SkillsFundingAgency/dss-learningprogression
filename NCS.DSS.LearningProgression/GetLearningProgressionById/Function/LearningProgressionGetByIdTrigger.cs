using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.Constants;
using NCS.DSS.LearningProgression.GetLearningProgressionById.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.LearningProgression.GetLearningProgressionById.Function
{
    public class LearningProgressionGetByIdTrigger
    {
        private const string RouteValue = "customers/{customerId}/learningprogressions/{learningProgressionId}";
        private const string FunctionName = "GETBYID";

        private readonly ILearningProgressionGetByIdService _learningProgressionByIdService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<LearningProgressionGetByIdTrigger> _logger;

        public LearningProgressionGetByIdTrigger(ILearningProgressionGetByIdService learningProgressionByIdService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            ILogger<LearningProgressionGetByIdTrigger> logger)
        {
            _learningProgressionByIdService = learningProgressionByIdService;
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
        [Display(Name = "GETBYID", Description = "Ability to retrieve an individual learning progression for the given customer.")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodGet, Route = RouteValue)]
            HttpRequest req, string customerId, string learningProgressionId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(LearningProgressionGetByIdTrigger));

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

            _logger.LogInformation("Attempting to see if customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _logger.LogInformation("Customer does not exist. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new BadRequestResult();
            }

            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            if (!Guid.TryParse(learningProgressionId, out var learnerProgressionGuid))
            {
                _logger.LogInformation("Unable to parse 'learnerProgressionId' to a GUID. Customer ID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerId, correlationGuid);
                return new BadRequestObjectResult(learnerProgressionGuid);
            }

            _logger.LogInformation("Attempting to retrieve LearningProgression for Customer. Customer GUID: {CustomerGuid}", customerGuid);
            var learningProgression = await _learningProgressionByIdService.GetLearningProgressionForCustomerAsync(customerGuid, learnerProgressionGuid);

            if (learningProgression != null)
            {
                _logger.LogInformation("LearningProgression successfully retrieved. Learning Progression ID: {LearningProgressionId}", learningProgression.LearningProgressionId.GetValueOrDefault());
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(LearningProgressionGetByIdTrigger));

                return new JsonResult(learningProgression, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            _logger.LogInformation("LearningProgression does not exist for Customer. Customer GUID: {CustomerGuid}", customerGuid);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(LearningProgressionGetByIdTrigger));
            return new NoContentResult();
        }
    }
}
