using Microsoft.Extensions.Logging;
using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.ServiceBus;
using System.Net;

namespace NCS.DSS.LearningProgression.PostLearningProgression.Service
{
    public class LearningProgressionPostTriggerService : ILearningProgressionPostTriggerService
    {
        private readonly ICosmosDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;
        private readonly ILogger<LearningProgressionPostTriggerService> _logger;

        public LearningProgressionPostTriggerService(ICosmosDBProvider documentDbProvider,
             IServiceBusClient serviceBusClient, 
             ILogger<LearningProgressionPostTriggerService> logger)
        {
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
            _logger = logger;
        }

        public async Task<Models.LearningProgression?> CreateLearningProgressionAsync(Models.LearningProgression? learningProgression)
        {

            if (learningProgression == null)
            {
                _logger.LogWarning("Unable to create LearningProgression. Invalid or NULL Object.");
                return null;
            }

            _logger.LogInformation($"Started creating LearningProgression for Customer ID: {learningProgression.CustomerId}");

            if (!learningProgression.LastModifiedDate.HasValue)
            {
                learningProgression.LastModifiedDate = DateTime.UtcNow;
            }

            var response = await _documentDbProvider.CreateLearningProgressionAsync(learningProgression);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                _logger.LogInformation($"Successfully created LearningProgression in CosmosDB. Response code [{response.StatusCode}]");
                return response.Resource;
            }

            _logger.LogWarning($"Unable to create LearningProgression in CosmosDB. Response code [{response.StatusCode}]");
            return null;
        }

        public async Task SendToServiceBusQueueAsync(Models.LearningProgression learningProgression, string reqUrl, Guid correlationId)
        {
            await _serviceBusClient.SendPostMessageAsync(learningProgression, reqUrl, correlationId);
        }

        public async Task<bool> DoesLearningProgressionExistForCustomer(Guid customerId)
        {
            return await _documentDbProvider.DoesLearningProgressionExistForCustomer(customerId);
        }

        public void SetIds(Models.LearningProgression learningProgression, Guid customerGuid, string? touchpointId)
        {
            learningProgression.LearningProgressionId = Guid.NewGuid();
            learningProgression.CustomerId = customerGuid;
            learningProgression.LastModifiedTouchpointId = touchpointId;
            learningProgression.CreatedBy = touchpointId;

            if (!learningProgression.DateProgressionRecorded.HasValue)
            {
                learningProgression.DateProgressionRecorded = DateTime.UtcNow;
            }

        }
    }
}
