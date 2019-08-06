using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.Models;
using NCS.DSS.LearningProgression.ServiceBus;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.GetLearningProgressionById.Service
{
    public class LearningProgressionGetByIdService : ILearningProgressionGetByIdService
    {
        private IDocumentDBProvider _documentDbProvider;
        LearningProgressionConfigurationSettings _learnerProgressConfigurationSettings;
        IServiceBusClient _serviceBusClient;

        public LearningProgressionGetByIdService(IDocumentDBProvider documentDbProvider,
            LearningProgressionConfigurationSettings learnerProgressConfigurationSettings, IServiceBusClient serviceBusClient)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async virtual Task<Models.LearningProgression> GetLearningProgressionForCustomerAsync(Guid customerId, Guid progressionProgressionId)
        {
            return await _documentDbProvider.GetLearningProgressionForCustomerAsync(customerId, progressionProgressionId);
        }

        public async virtual Task SendToServiceBusQueueAsync(Models.LearningProgression learningProgression, Guid customerId, string reqUrl)
        {
            await _serviceBusClient.SendPostMessageAsync(learningProgression, reqUrl);
        }
    }
}
