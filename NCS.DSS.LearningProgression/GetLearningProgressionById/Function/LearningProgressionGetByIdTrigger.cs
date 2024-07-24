using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.Constants;
using NCS.DSS.LearningProgression.GetLearningProgressionById.Service;

namespace NCS.DSS.LearningProgression.GetLearningProgressionById.Function
{
    public class LearningProgressionGetByIdTrigger
    {
        private const string RouteValue = "customers/{customerId}/learningprogressions/{LearningProgressionId}";
        private const string FunctionName = "GetById";

        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILearningProgressionGetByIdService _learningProgressionByIdService;
        private readonly IJsonHelper _jsonHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<LearningProgressionGetByIdTrigger> _logger;
                
        public LearningProgressionGetByIdTrigger(
            
            IHttpRequestHelper httpRequestHelper,
            ILearningProgressionGetByIdService learningProgressionByIdService,
            IJsonHelper jsonHelper,
            IResourceHelper resourceHelper,
            ILogger<LearningProgressionGetByIdTrigger> logger)
        {           
            _httpRequestHelper = httpRequestHelper;
            _learningProgressionByIdService = learningProgressionByIdService;
            _jsonHelper = jsonHelper;
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
        [Display(Name = "Get", Description = "Ability to retrieve an individual learning progression for the given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodGet, Route = RouteValue)]
            HttpRequest req, string customerId, string learningProgressionId)
        {
            _logger.LogInformation("Getting Learning Progression of ID [{0}] for Customer ID [{1}]", learningProgressionId, customerId);
            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            var guidHelper = new GuidHelper();
            var correlationGuid = guidHelper.ValidateAndGetGuid(correlationId);

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("CorrelationId: {0} Unable to locate 'TouchpointId' in request header.", correlationGuid);
                return new BadRequestResult();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
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

            if (!Guid.TryParse(learningProgressionId, out var learnerProgressionGuid))
            {
                _logger.LogWarning("CorrelationId: {0} Unable to parse 'learnerProgressionID' to a Guid: {1}", correlationGuid, learnerProgressionGuid);
                return new BadRequestObjectResult(learnerProgressionGuid);
            }

            var learningProgression = await _learningProgressionByIdService.GetLearningProgressionForCustomerAsync(customerGuid, learnerProgressionGuid);
            if(learningProgression == null)
            {
                _logger.LogWarning("CorrelationId: {0} No Content", correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("CorrelationId: {0} Ok", correlationGuid);

            return new OkObjectResult(
                _jsonHelper.SerializeObjectAndRenameIdProperty(learningProgression, "id", "LearningProgressionId"));
        }
    }
}
