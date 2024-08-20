using DFC.JSON.Standard;
using Microsoft.Extensions.Logging;
using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.Models;
using NCS.DSS.LearningProgression.ServiceBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.PatchLearningProgression.Service
{
    public class LearningProgressionPatchTriggerService : ILearningProgressionPatchTriggerService
    {
        private readonly IJsonHelper _jsonHelper;
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;
        private readonly ILogger<LearningProgressionPatchTriggerService> _log;
        
        public LearningProgressionPatchTriggerService(IJsonHelper jsonHelper, IDocumentDBProvider documentDbProvider, IServiceBusClient serviceBusClient, ILogger<LearningProgressionPatchTriggerService> log)
        {
            _jsonHelper = jsonHelper;
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
            _log = log;
        }

        public async Task<string> GetLearningProgressionForCustomerToPatchAsync(Guid customerId, Guid learningProgressionId)
        {
            var learningProgressionAsString = await _documentDbProvider.GetLearningProgressionForCustomerToPatchAsync(customerId, learningProgressionId);

            return learningProgressionAsString;
        }

        public string PatchLearningProgressionAsync(string learningProgressionAsJson, Models.LearningProgressionPatch learningProgressionPatch)
        {
            try
            {
                var learningProgressionAsJsonObject = JObject.Parse(learningProgressionAsJson);

                if (learningProgressionPatch.DateProgressionRecorded.HasValue)
                    _jsonHelper.UpdatePropertyValue(learningProgressionAsJsonObject["DateProgressionRecorded"], learningProgressionPatch.DateProgressionRecorded);

                if (learningProgressionPatch.CurrentLearningStatus.HasValue)
                    _jsonHelper.UpdatePropertyValue(learningProgressionAsJsonObject["CurrentLearningStatus"], learningProgressionPatch.CurrentLearningStatus);

                if (learningProgressionPatch.LearningHours.HasValue)
                    _jsonHelper.UpdatePropertyValue(learningProgressionAsJsonObject["LearningHours"], learningProgressionPatch.LearningHours);

                if (learningProgressionPatch.DateLearningStarted.HasValue)
                    _jsonHelper.UpdatePropertyValue(learningProgressionAsJsonObject["DateLearningStarted"], learningProgressionPatch.DateLearningStarted);

                if (learningProgressionPatch.CurrentQualificationLevel.HasValue)
                    _jsonHelper.UpdatePropertyValue(learningProgressionAsJsonObject["CurrentQualificationLevel"], learningProgressionPatch.CurrentQualificationLevel);

                if (learningProgressionPatch.DateQualificationLevelAchieved.HasValue)
                    _jsonHelper.UpdatePropertyValue(learningProgressionAsJsonObject["DateQualificationLevelAchieved"], learningProgressionPatch.DateQualificationLevelAchieved);

                if (!string.IsNullOrEmpty(learningProgressionPatch.LastLearningProvidersUKPRN))
                    _jsonHelper.UpdatePropertyValue(learningProgressionAsJsonObject["LastLearningProvidersUKPRN"], learningProgressionPatch.LastLearningProvidersUKPRN);

                if (learningProgressionPatch.LastModifiedDate.HasValue)
                    _jsonHelper.UpdatePropertyValue(learningProgressionAsJsonObject["LastModifiedDate"], learningProgressionPatch.LastModifiedDate);

                if (!string.IsNullOrEmpty(learningProgressionPatch.LastModifiedTouchpointId))
                    _jsonHelper.UpdatePropertyValue(learningProgressionAsJsonObject["LastModifiedTouchpointId"], learningProgressionPatch.LastModifiedTouchpointId);

                _log.LogInformation($"Successfully Fetched PATCH Json Object.");

                return learningProgressionAsJsonObject.ToString();
            }
            catch (JsonReaderException ex)
            {
                _log.LogWarning($"Failed to PATCH Json Object. Error: [{ex.Message}] and Stack Trace [{ex.StackTrace}]");
                return null;
            }
        }

        public async Task<Models.LearningProgression> UpdateCosmosAsync(string learningProgressionAsJson, Guid learningProgressionId)
        {
            if (string.IsNullOrEmpty(learningProgressionAsJson))
            {
                _log.LogWarning($"Failed to Update in Cosmos DB. Json Object Can not be Null or Empty.");
                return null;
            }

            var response = await _documentDbProvider.UpdateLearningProgressionAsync(learningProgressionAsJson, learningProgressionId);
            var responseStatusCode = response?.StatusCode;


            if( responseStatusCode == HttpStatusCode.OK )
            {
                _log.LogInformation($"Successfully Updated in Cosmos DB. Response Code [{responseStatusCode}]");
                return (dynamic)response.Resource;
            }

            _log.LogWarning($"Failed to Update in Cosmos DB. Response Code [{responseStatusCode}]");
            return null;
        }

        public async Task SendToServiceBusQueueAsync(Models.LearningProgression learningProgression, Guid customerId, string reqUrl, Guid correlationId, ILogger log)
        {
            await _serviceBusClient.SendPatchMessageAsync(learningProgression, customerId, reqUrl, correlationId, log);
        }

        public bool DoesLearningProgressionExistForCustomer(Guid customerId)
        {
            return _documentDbProvider.DoesLearningProgressionExistForCustomer(customerId);
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _documentDbProvider.DoesCustomerResourceExist(customerId);
        }

        public void SetIds(LearningProgressionPatch learningProgressionPatchRequest, Guid learningProgressionGuid, string touchpointId)
        {
            learningProgressionPatchRequest.LastModifiedTouchpointId = touchpointId;
            learningProgressionPatchRequest.LearningProgressionId = learningProgressionGuid;

            if (!learningProgressionPatchRequest.LastModifiedDate.HasValue)
            {
                learningProgressionPatchRequest.LastModifiedDate = DateTime.UtcNow;
            }
        }
    }
}
