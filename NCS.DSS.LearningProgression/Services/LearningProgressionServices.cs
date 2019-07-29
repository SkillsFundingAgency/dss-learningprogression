using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.LearningProgression.ServiceBus;
using NCS.DSS.LearningProgression.Cosmos.Provider;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using DFC.JSON.Standard;
using Newtonsoft.Json;

namespace NCS.DSS.LearningProgression.Services
{
    public class LearningProgressionServices : ILearningProgressionServices
    {
        private IDocumentDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;
        private readonly LearnerProgressConfigurationSettings _learnerProgressConfigurationSettings;
        private readonly ILogger _logger;
        private readonly IJsonHelper _jsonHelper;

        public LearningProgressionServices(
            IDocumentDBProvider documentDbProvider,
            IServiceBusClient serviceBusClient,
            LearnerProgressConfigurationSettings learnerProgressConfigurationSettings,
            ILogger logger,
            IJsonHelper jsonHelper
            )
        {
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
            _learnerProgressConfigurationSettings = learnerProgressConfigurationSettings;
            _logger = logger;
            _jsonHelper = jsonHelper;
        }

        public bool DoesLearningProgressionExistForCustomer(Guid customerId)
        {
            return _documentDbProvider.DoesLearningProgressionExistForCustomer(customerId);
        }

        public async virtual Task<Models.LearningProgression> CreateLearningProgressionAsync(Models.LearningProgression learningProgression)
        {
            if (learningProgression == null)
            {
                _logger.LogInformation("LearningProgression is null.");
                return null;
            }

            learningProgression.LearningProgressionId = Guid.NewGuid();

            if (!learningProgression.LastModifiedDate.HasValue)
            {
                learningProgression.LastModifiedDate = DateTime.UtcNow;
            }

            var response = await _documentDbProvider.CreateLearningProgressionAsync(learningProgression);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : (Guid?)null;
        }

        public async virtual Task SendToServiceBusQueueAsync(Models.LearningProgression learningProgression, string reqUrl)
        {
            await _serviceBusClient.SendPostMessageAsync(learningProgression, reqUrl, _learnerProgressConfigurationSettings, _logger);
        }

        public async virtual Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _documentDbProvider.DoesCustomerResourceExist(customerId);
        }

        public async virtual Task<List<Models.LearningProgression>> GetLearningProgressionsForCustomerAsync(Guid customerId)
        {
            return await _documentDbProvider.GetLearningProgressionsForCustomerAsync(customerId);
        }

        public async virtual Task<Models.LearningProgression> GetLearningProgressionForCustomerAsync(Guid customerId, Guid progressionProgressionId)
        {
            return await _documentDbProvider.GetLearningProgressionForCustomerAsync(customerId, progressionProgressionId);
        }

        public async virtual Task<string> GetLearningProgressionForCustomerToPatchAsync(Guid customerId, Guid learningProgressionId)
        {
            var learningProgressionAsString = await _documentDbProvider.GetLearningProgressionForCustomerToPatchAsync(customerId, learningProgressionId);

            return learningProgressionAsString;
        }

        public virtual string PatchLearningProgressionAsync(string learningProgressionAsJson, Models.LearningProgressionPatch learningProgressionPatch)
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

                if (!string.IsNullOrEmpty(learningProgressionPatch.LastModifiedTouchpointID))
                    _jsonHelper.UpdatePropertyValue(learningProgressionAsJsonObject["LastModifiedTouchpointID"], learningProgressionPatch.LastModifiedTouchpointID);

                if (!string.IsNullOrEmpty(learningProgressionPatch.CreatedBy))
                {
                    if (learningProgressionAsJsonObject["CreatedBy"] == null)
                        _jsonHelper.CreatePropertyOnJObject(learningProgressionAsJsonObject, "CreatedBy", learningProgressionPatch.CreatedBy);
                    else
                        _jsonHelper.UpdatePropertyValue(learningProgressionAsJsonObject["CreatedBy"], learningProgressionPatch.CreatedBy);
                }

                return learningProgressionAsJsonObject.ToString();
            }
            catch (JsonReaderException ex)
            {
                // todo
                return null;
            }
        }

        public virtual void SetIds(Models.LearningProgression learningProgression, Guid customerGuid, string touchpointId)
        {
            learningProgression.LearningProgressionId = Guid.NewGuid();
            learningProgression.CustomerId = customerGuid;
            learningProgression.LastModifiedTouchpointID = touchpointId;
            learningProgression.CreatedBy = touchpointId;
        }

        public virtual async Task<Models.LearningProgression> UpdateCosmosAsync(string learningProgressionAsJson, Guid learningProgressionId)
        {
            if (string.IsNullOrEmpty(learningProgressionAsJson))
            {
                return null;
            }

            var response = await _documentDbProvider.UpdateLearningProgressionAsync(learningProgressionAsJson, learningProgressionId);
            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic)response.Resource : null;
        }
    }
}