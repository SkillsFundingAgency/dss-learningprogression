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
using Microsoft.Azure.Documents;
using System;
using DFC.HTTP.Standard;
using NCS.DSS.LearningProgression.Services;
using Newtonsoft.Json;
using DFC.Common.Standard.GuidHelper;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.CosmosDocumentClient;
using NCS.DSS.LearningProgression.Validators;
using System.Linq;

namespace NCS.DSS.LearningProgression
{
    public class LearningProgressionPostTrigger
    {
        const string RouteValue = "customers/{customerId}/LearningProgessions";
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILearningProgressionServices _learningProgressionServices;
        private readonly IJsonHelper _jsonHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly LearnerProgressConfigurationSettings _learnerProgressConfigurationSettings;
        private ICosmosDocumentClient _cosmosDocumentClient;
        private IDocumentClient _documentClient;
        private Models.LearningProgression learningProgression;
        private IValidate _validate;

        public LearningProgressionPostTrigger(
            LearnerProgressConfigurationSettings learnerProgressConfigurationSettings,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IHttpRequestHelper httpRequestHelper,
            ILearningProgressionServices learningProgressionServices,
            IJsonHelper jsonHelper,
            IResourceHelper resourceHelper,
            ICosmosDocumentClient cosmosDocumentClient,
            IValidate validate
            )
        {
            _learnerProgressConfigurationSettings = learnerProgressConfigurationSettings;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _httpRequestHelper = httpRequestHelper;
            _learningProgressionServices = learningProgressionServices;
            _jsonHelper = jsonHelper;
            _resourceHelper = resourceHelper;
            _cosmosDocumentClient = cosmosDocumentClient;
            _validate = validate;
        }

        [FunctionName("post")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Learning progression created.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Post request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this learning progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Learning progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.LearningProgression), (int)HttpStatusCode.OK)]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodPost, Route = RouteValue)]HttpRequest req, ILogger logger, string customerId)
        {
            Guid correlationGuid = Guid.Empty;

            try
            {
                logger.LogInformation("C# HTTP LearningProgressionPostTrigger is executing a request.");
                _documentClient = _cosmosDocumentClient.GetDocumentClient();

                var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

                var guidHelper = new GuidHelper();
                correlationGuid = guidHelper.ValidateGuid(correlationId);

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

                var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

                if (isCustomerReadOnly)
                {
                    logger.LogInformation($"Customer is readonly with customerId {customerGuid}, correlationId {correlationGuid}.");
                    return _httpResponseMessageHelper.Forbidden(customerGuid);
                }

                var doesLearningProgressionExist = _learningProgressionServices.DoesLearningProgressionExistForCustomer(customerGuid);
                if (doesLearningProgressionExist)
                {
                    logger.LogInformation($"Learning progression details already exists for customerId {customerGuid}, correlationId {correlationGuid}.");
                    return _httpResponseMessageHelper.Conflict();
                }

                logger.LogInformation($"Attempt to get resource from body of the request correlationId {correlationGuid}.");
                learningProgression = await _httpRequestHelper.GetResourceFromRequest<Models.LearningProgression>(req);
                _learningProgressionServices.SetIds(learningProgression, customerGuid, touchpointId);

                var errors = _validate.ValidateResource(learningProgression);

                if (errors != null && errors.Any())
                {
                    logger.LogInformation($"validation errors with resource correlationId {correlationGuid}");
                    return _httpResponseMessageHelper.UnprocessableEntity(errors);
                }

                var learningProgressionResult = await _learningProgressionServices.CreateLearningProgressionAsync(learningProgression);
                if (learningProgressionResult == null)
                {
                    logger.LogInformation($"learning progression does not exists for customerId {customerGuid}, correlationId {correlationGuid}.");
                    return _httpResponseMessageHelper.BadRequest(customerGuid);
                }

                logger.LogInformation($"Learning Progression details already exists for customerId {customerGuid}, correlationId {correlationGuid}.");
                // todo uncomment the line below
                // await _learningProgressionServices.SendToServiceBusQueueAsync(learningProgression, ApimURL);

                var learningProgressionAsJson = _jsonHelper.SerializeObjectAndRenameIdProperty(learningProgression, "id", "LearningProgressionId");
                return _httpResponseMessageHelper.Ok(learningProgressionAsJson);
            }
            catch (JsonException ex)
            {
                logger.LogError($"Unable to retrieve body from req {correlationGuid}.", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }
        }
    }
}