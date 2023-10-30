using Microsoft.Extensions.Logging;
using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.Models;
using NCS.DSS.LearningProgression.ServiceBus;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.PostLearningProgression.Service
{
    public class LearningProgressionPostTriggerService : ILearningProgressionPostTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;
        private ILogger<LearningProgressionPostTriggerService> _log;

        public LearningProgressionPostTriggerService(IDocumentDBProvider documentDbProvider,
             IServiceBusClient serviceBusClient, ILogger<LearningProgressionPostTriggerService> log)
        {
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
            _log = log;            
        }

        public async Task<Models.LearningProgression> CreateLearningProgressionAsync(Models.LearningProgression learningProgression)
        {
           
            if (learningProgression == null)
            {
                _log.LogWarning($"Unable to Create Learning Progression. Invalid or NULL Object.");
                return null;
            }

            _log.LogInformation($"Started Creating Learning Progression for [{learningProgression.CustomerId}]");

            learningProgression.LearningProgressionId = Guid.NewGuid();

            if (!learningProgression.LastModifiedDate.HasValue)
            {
                learningProgression.LastModifiedDate = DateTime.UtcNow;
            }

            var response = await _documentDbProvider.CreateLearningProgressionAsync(learningProgression);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                _log.LogInformation($"Successfully Created Learning Progression in Cosmos DB. Response Code [{response.StatusCode}]");
                return (dynamic)response.Resource;
            }

            _log.LogWarning($"Unable to Create Learning Progression in Cosmos DB. Response Code [{response.StatusCode}]");
            return null;
        }

        public async Task SendToServiceBusQueueAsync(Models.LearningProgression learningProgression, string reqUrl, Guid correlationId, ILogger log)
        {
            await _serviceBusClient.SendPostMessageAsync(learningProgression, reqUrl, correlationId, log);
        }

        public bool DoesLearningProgressionExistForCustomer(Guid customerId)
        {
            return _documentDbProvider.DoesLearningProgressionExistForCustomer(customerId);
        }

        public void SetIds(Models.LearningProgression learningProgression, Guid customerGuid, string touchpointId)
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
