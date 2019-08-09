using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.ServiceBus;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.GetLearningProgressionById.Service
{
    public class LearningProgressionGetByIdService : ILearningProgressionGetByIdService
    {
        private readonly IDocumentDBProvider _documentDbProvider;
        readonly IServiceBusClient _serviceBusClient;

        public LearningProgressionGetByIdService(IDocumentDBProvider documentDbProvider, IServiceBusClient serviceBusClient)
        {
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
        }

        public async virtual Task<Models.LearningProgression> GetLearningProgressionForCustomerAsync(Guid customerId, Guid progressionProgressionId)
        {
            return await _documentDbProvider.GetLearningProgressionForCustomerAsync(customerId, progressionProgressionId);
        }

        public async virtual Task SendToServiceBusQueueAsync(Models.LearningProgression learningProgression, string reqUrl)
        {
            await _serviceBusClient.SendPostMessageAsync(learningProgression, reqUrl);
        }
    }
}
