using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.LearningProgression.Constants;
using DFC.HTTP.Standard;
using NCS.DSS.LearningProgression.Services;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using Microsoft.Azure.Documents;
using DFC.Common.Standard.GuidHelper;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using NCS.DSS.LearningProgression.CosmosDocumentClient;

namespace NCS.DSS.LearningProgression
{
    public class LearningProgressionGetByIdTrigger
    {
        const string RouteValue = "customers/{customerId}/learningprogessions/{LearningProgessionId}";
        const string FunctionName = "run";
        private string _cosmosDBConnectionString = "CosmosDBConnectionString";
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILearningProgressionServices _learningProgressionServices;
        private readonly IJsonHelper _jsonHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly LearnerProgressConfigurationSettings _learnerProgressConfigurationSettings;
        private ICosmosDocumentClient _cosmosDocumentClient;
        private IDocumentClient _documentClient;
        private Models.LearningProgression learningProgression;

        public LearningProgressionGetByIdTrigger(  
            LearnerProgressConfigurationSettings learnerProgressConfigurationSettings,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IHttpRequestHelper httpRequestHelper,
            ILearningProgressionServices learningProgressionServices,
            IJsonHelper jsonHelper,
            IResourceHelper resourceHelper,
            ICosmosDocumentClient cosmosDocumentClient
            )
        {
            _learnerProgressConfigurationSettings = learnerProgressConfigurationSettings;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _httpRequestHelper = httpRequestHelper;
            _learningProgressionServices = learningProgressionServices;
            _jsonHelper = jsonHelper;
            _resourceHelper = resourceHelper;
            _cosmosDocumentClient = cosmosDocumentClient;
        }

        [FunctionName("getById")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodGet, Route = RouteValue)]
            HttpRequest req, ILogger logger, string customerId, string learningProgressionId)
        {
            try
            {
                logger.LogInformation("C# HTTP LearningProgressionGetTrigger is executing a request.");
                _documentClient = _cosmosDocumentClient.GetDocumentClient();

                var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

                var t = new GuidHelper();
                var correlationGuid = t.ValidateGuid(correlationId);

                var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
                if (string.IsNullOrEmpty(touchpointId))
                {
                    logger.LogInformation("Unable to locate 'TouchpointId' in request header.");
                    return _httpResponseMessageHelper.BadRequest();
                }

                var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
                if (string.IsNullOrEmpty(ApimURL))
                {
                    logger.LogInformation("Unable to locate 'apimurl' in request header");
                    return _httpResponseMessageHelper.BadRequest();
                }

                if (!Guid.TryParse(customerId, out var customerGuid))
                {
                    logger.LogInformation($"Unable to parse 'customerId' to a Guid: {customerId}");
                    return _httpResponseMessageHelper.BadRequest(customerGuid);
                }

                if (!await _learningProgressionServices.DoesCustomerExist(customerGuid))
                {
                    logger.LogInformation("Bad request");
                    return _httpResponseMessageHelper.BadRequest();
                }

                if (!Guid.TryParse(learningProgressionId, out var learnerProgressionGuid))
                {
                    logger.LogInformation($"Unable to parse 'learnerProgressioniD' to a Guid: {learningProgressionId}");
                    return _httpResponseMessageHelper.BadRequest(learnerProgressionGuid);
                }

                var learningProgression = await _learningProgressionServices.GetLearningProgressionForCustomerAsync(customerGuid, learnerProgressionGuid);
                return _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(learningProgression, "id", "learningProgressionId"));
            }
            catch (JsonException ex)
            {
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }
        }
    }
}
