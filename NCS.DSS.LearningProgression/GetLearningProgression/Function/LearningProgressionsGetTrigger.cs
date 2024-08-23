using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.Constants;
using NCS.DSS.LearningProgression.GetLearningProgression.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.LearningProgression.GetLearningProgression.Function
{
    public class LearningProgressionsGetTrigger
    {
        private const string RouteValue = "customers/{customerId}/LearningProgressions";
        private const string FunctionName = "Get";
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILearningProgressionsGetTriggerService _learningProgressionsGetTriggerService;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<LearningProgressionsGetTrigger> _logger;

        public LearningProgressionsGetTrigger(

            IHttpRequestHelper httpRequestHelper,
            ILearningProgressionsGetTriggerService learningProgressionsGetTriggerService,
            IResourceHelper resourceHelper,
            ILogger<LearningProgressionsGetTrigger> logger
            )
        {
            _httpRequestHelper = httpRequestHelper;
            _learningProgressionsGetTriggerService = learningProgressionsGetTriggerService;
            _resourceHelper = resourceHelper;
            _logger = logger;
        }

        [Function(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Learning progression found.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this learning progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Learning progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.LearningProgression), (int)HttpStatusCode.OK)]
        [Display(Name = "Get", Description = "Ability to return all learning progressions for the given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodGet, Route = RouteValue)]
            HttpRequest req, string customerId)
        {
            _logger.LogInformation("Getting Learning Progression for Customer ID [{0}]", customerId);

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

            var learningProgression = await _learningProgressionsGetTriggerService.GetLearningProgressionsForCustomerAsync(customerGuid);

            if (learningProgression == null)
            {
                _logger.LogWarning("CorrelationId: {0} Bad request", correlationGuid);
                return new NoContentResult();
            }

            _logger.LogInformation("CorrelationId: {0} Ok", correlationGuid);

            if (learningProgression.Count == 1)
            {
                return new JsonResult(learningProgression[0], new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            return new JsonResult(learningProgression, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
