using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.LearningProgression.Constants;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using Microsoft.Azure.Documents;
using DFC.Common.Standard.GuidHelper;
using System.Net.Http;
using System;
using NCS.DSS.LearningProgression.CosmosDocumentClient;
using System.Net;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Mvc;
using DFC.Common.Standard.Logging;
using NCS.DSS.LearningProgression.GetLearningProgressionById.Service;
using NCS.DSS.LearningProgression.Models;

namespace NCS.DSS.LearningProgression
{
    public class LearningProgressionGetByIdTrigger
    {
        const string RouteValue = "customers/{customerId}/learningprogessions/{LearningProgessionId}";
        const string FunctionName = "getById";
        private string _cosmosDBConnectionString = "CosmosDBConnectionString";
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILearningProgressionGetByIdService _learningProgressionByIdService;
        private readonly IJsonHelper _jsonHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILoggerHelper _loggerHelper;
        private readonly LearningProgressionConfigurationSettings _learningProgressionConfigurationSettings;
        private ICosmosDocumentClient _cosmosDocumentClient;
        private IDocumentClient _documentClient;
        private Models.LearningProgression learningProgression;


        public LearningProgressionGetByIdTrigger(
            LearningProgressionConfigurationSettings learnerProgressConfigurationSettings,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IHttpRequestHelper httpRequestHelper,
            ILearningProgressionGetByIdService learningProgressionByIdService,
            IJsonHelper jsonHelper,
            IResourceHelper resourceHelper,
            ICosmosDocumentClient cosmosDocumentClient,
            ILoggerHelper loggerHelper
            )
        {
            _learningProgressionConfigurationSettings = learnerProgressConfigurationSettings;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _httpRequestHelper = httpRequestHelper;
            _learningProgressionByIdService = learningProgressionByIdService;
            _jsonHelper = jsonHelper;
            _resourceHelper = resourceHelper;
            _loggerHelper = loggerHelper;
            _cosmosDocumentClient = cosmosDocumentClient;
        }

        [FunctionName(FunctionName)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Learning progression created.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Post request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this learning progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Learning progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.LearningProgression), (int)HttpStatusCode.OK)]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodGet, Route = RouteValue)]
            HttpRequest req, ILogger logger, string customerId, string LearningProgessionId)
        {
            _loggerHelper.LogMethodEnter(logger);
            _documentClient = _cosmosDocumentClient.GetDocumentClient();

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

            if (!await _resourceHelper.DoesCustomerExist(customerGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, "Bad request");
                return _httpResponseMessageHelper.BadRequest();
            }

            if (!Guid.TryParse(LearningProgessionId, out var learnerProgressionGuid))
            {
                _loggerHelper.LogInformationMessage(logger, correlationGuid, $"Unable to parse 'learnerProgressioniD' to a Guid: {LearningProgessionId}");
                return _httpResponseMessageHelper.BadRequest(learnerProgressionGuid);
            }

            var learningProgression = await _learningProgressionByIdService.GetLearningProgressionForCustomerAsync(customerGuid, learnerProgressionGuid);

            _loggerHelper.LogMethodExit(logger);

            return learningProgression == null ?
            _httpResponseMessageHelper.NoContent(customerGuid) :
            _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(learningProgression, "id", "learningProgressionId"));
        }
    }
}
