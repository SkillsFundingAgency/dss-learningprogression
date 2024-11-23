using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.ServiceBus;

namespace NCS.DSS.LearningProgression.GetLearningProgression.Service
{
    public class LearningProgressionsGetTriggerService : ILearningProgressionsGetTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public LearningProgressionsGetTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async virtual Task<List<Models.LearningProgression>> GetLearningProgressionsForCustomerAsync(Guid customerId)
        {
            return await _documentDbProvider.GetLearningProgressionsForCustomerAsync(customerId);
        }
    }
}
