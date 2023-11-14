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
using NCS.DSS.LearningProgression.GetLearningProgressionById.Service;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression
{
    public class LearningProgressionGetByIdTrigger
    {
        const string RouteValue = "customers/{customerId}/learningprogressions/{LearningProgressionId}";
        const string FunctionName = "GetById";

        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILearningProgressionGetByIdService _learningProgressionByIdService;
        private readonly IJsonHelper _jsonHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILoggerHelper _loggerHelper;
                
        public LearningProgressionGetByIdTrigger(
            
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IHttpRequestHelper httpRequestHelper,
            ILearningProgressionGetByIdService learningProgressionByIdService,
            IJsonHelper jsonHelper,
            IResourceHelper resourceHelper,
            ILoggerHelper loggerHelper
            )
        {           
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _httpRequestHelper = httpRequestHelper;
            _learningProgressionByIdService = learningProgressionByIdService;
            _jsonHelper = jsonHelper;
            _resourceHelper = resourceHelper;
            _loggerHelper = loggerHelper;
        }

        [FunctionName(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Learning progression found.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this learning progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Learning progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.LearningProgression), (int)HttpStatusCode.OK)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual learning progression for the given customer.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodGet, Route = RouteValue)]
            HttpRequest req, ILogger logger, string customerId, string LearningProgressionId)
        {
            _loggerHelper.LogMethodEnter(logger);
            logger.LogInformation($"Getting Learning Progression of ID [{LearningProgressionId}] for Customer ID [{customerId}]");
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

            if (!Guid.TryParse(LearningProgressionId, out var learnerProgressionGuid))
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, $"Unable to parse 'learnerProgressioniD' to a Guid: {LearningProgressionId}");
                return _httpResponseMessageHelper.BadRequest(learnerProgressionGuid);
            }

            var learningProgression = await _learningProgressionByIdService.GetLearningProgressionForCustomerAsync(customerGuid, learnerProgressionGuid);
            if(learningProgression == null)
            {
                _loggerHelper.LogWarningMessage(logger, correlationGuid, "No Content");
                _loggerHelper.LogMethodExit(logger);

                return _httpResponseMessageHelper.NoContent(customerGuid);
            }
            _loggerHelper.LogInformationMessage(logger, correlationGuid, "Ok");
            _loggerHelper.LogMethodExit(logger);

            return _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(learningProgression, "id", "LearningProgressionId"));
        }
    }
}
