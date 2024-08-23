using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.ServiceBus;

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
    }
}
