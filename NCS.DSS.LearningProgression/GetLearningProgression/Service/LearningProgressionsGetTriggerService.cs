using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.ServiceBus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.GetLearningProgression.Service
{
    public class LearningProgressionsGetTriggerService : ILearningProgressionsGetTriggerService
    {
        private IDocumentDBProvider _documentDbProvider;
        LearnerProgressConfigurationSettings _learnerProgressConfigurationSettings;
        IServiceBusClient _serviceBusClient;

        public LearningProgressionsGetTriggerService(IDocumentDBProvider documentDbProvider,
            LearnerProgressConfigurationSettings learnerProgressConfigurationSettings, IServiceBusClient serviceBusClient)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async virtual Task<List<Models.LearningProgression>> GetLearningProgressionsForCustomerAsync(Guid customerId)
        {
            return await _documentDbProvider.GetLearningProgressionsForCustomerAsync(customerId);
        }

        public async virtual Task SendToServiceBusQueueAsync(Models.LearningProgression learningProgression, Guid customerId, string reqUrl)
        {
            await _serviceBusClient.SendPostMessageAsync(learningProgression, reqUrl, _learnerProgressConfigurationSettings);
        }
    }
}
