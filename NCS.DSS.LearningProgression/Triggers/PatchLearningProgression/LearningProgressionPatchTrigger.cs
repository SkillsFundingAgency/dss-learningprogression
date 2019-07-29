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
using NCS.DSS.LearningProgression.Models;

namespace NCS.DSS.LearningProgression
{
    public class LearningProgressionPatchTrigger
    {
        const string RouteValue = "customers/{customerId}/learningprogessions/{LearningProgessionId}";
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILearningProgressionServices _learningProgressionServices;
        private readonly IJsonHelper _jsonHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly LearnerProgressConfigurationSettings _learnerProgressConfigurationSettings;
        private ICosmosDocumentClient _cosmosDocumentClient;
        private IDocumentClient _documentClient;
        private LearningProgressionPatch learningProgressionPatch;
        private IValidate _validate;

        public LearningProgressionPatchTrigger(
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

        [FunctionName("patch")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Learning progression created.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Customer Resource does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Post request is malformed.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access to this learning progression.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)422, Description = "Learning progression validation error(s).", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.LearningProgression), (int)HttpStatusCode.OK)]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, Constant.MethodPatch, Route = RouteValue)]HttpRequest req, ILogger logger, string customerId, string learningProgressionId)
        {
            Guid correlationGuid = Guid.Empty;

            try
            {
                logger.LogInformation("C# HTTP LearningProgressionPatchTrigger is executing a request.");
                _documentClient = _cosmosDocumentClient.GetDocumentClient();

                correlationGuid = GetCorrelationId(req);

                var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
                if (string.IsNullOrEmpty(touchpointId))
                {
                    logger.LogInformation("Unable to locate 'TouchpointId' in request header.");
                    return _httpResponseMessageHelper.BadRequest();
                }

                var subContractorId = _httpRequestHelper.GetDssSubcontractorId(req);

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

                if (!Guid.TryParse(learningProgressionId, out var learnerProgressionGuid))
                {
                    logger.LogInformation($"Unable to parse 'learnerProgressionId' to a Guid: {learnerProgressionGuid}");
                    return _httpResponseMessageHelper.BadRequest(learnerProgressionGuid);
                }

                // jet the json of the patch request
                learningProgressionPatch = await _httpRequestHelper.GetResourceFromRequest<Models.LearningProgressionPatch>(req);

                if (learningProgressionPatch == null)
                {
                    logger.LogInformation($"A patch body was not provided. CorrelationId: {correlationGuid}.");
                    return _httpResponseMessageHelper.NoContent();
                }

                if (await _resourceHelper.IsCustomerReadOnly(customerGuid))
                {
                    logger.LogInformation($"Customer is readonly with customerId {customerGuid}, correlationId {correlationGuid}.");
                    return _httpResponseMessageHelper.Forbidden(customerGuid);
                }

                if (!await _learningProgressionServices.DoesCustomerExist(customerGuid))
                {
                    logger.LogInformation("Bad request");
                    return _httpResponseMessageHelper.BadRequest();
                }

                //if (!_learningProgressionServices.DoesLearningProgressionExistForCustomer(customerGuid))
                //{
                //    logger.LogInformation($"Learning progression does not exist for customerId {customerGuid}, correlationId {correlationGuid}.");
                //    return _httpResponseMessageHelper.NoContent();
                //}

                var currentLearningProgressionAsJson = await _learningProgressionServices.GetLearningProgressionForCustomerToPatchAsync(customerGuid, learnerProgressionGuid);

                // _learningProgressionServices.SetIds(learningProgressionPatch, customerGuid, touchpointId, subContractorId);

                var patchLearningProgression = _learningProgressionServices.PatchLearningProgressionAsync(currentLearningProgressionAsJson, learningProgressionPatch);
                if (patchLearningProgression == null)
                {
                    logger.LogInformation($"learning progression patching failed {customerGuid}, correlationId {correlationGuid}.");
                    return _httpResponseMessageHelper.BadRequest(customerGuid);
                }

                //// look at the way validate is done for patching
                //var errors = _validate.ValidateResource(patchLearningProgression);
                //if (errors != null && errors.Any())
                //{
                //    logger.LogInformation($"validation errors with resource customerId {customerGuid} correlationId {correlationGuid}.");
                //    return _httpResponseMessageHelper.UnprocessableEntity(errors);
                //}

                // todo uncomment the line below
                // await _learningProgressionServices.SendToServiceBusQueueAsync(learningProgression, ApimURL);

                var learningProgressionAsJson = _jsonHelper.SerializeObjectAndRenameIdProperty(learningProgressionPatch, "id", "LearningProgressionId");
                return _httpResponseMessageHelper.Ok(learningProgressionAsJson);
            }
            catch (JsonException ex)
            {
                logger.LogError($"Unable to retrieve body from req {correlationGuid}.", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }
        }

        private Guid GetCorrelationId(HttpRequest req)
        {
            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            var guidHelper = new GuidHelper();
            var correlationGuid = guidHelper.ValidateGuid(correlationId);
            return correlationGuid;
        }
    }
}