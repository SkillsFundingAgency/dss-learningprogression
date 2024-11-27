using NCS.DSS.LearningProgression.Cosmos.Provider;

namespace NCS.DSS.LearningProgression.GetLearningProgression.Service
{
    public class LearningProgressionsGetTriggerService : ILearningProgressionsGetTriggerService
    {
        private readonly ICosmosDBProvider _documentDbProvider;

        public LearningProgressionsGetTriggerService(ICosmosDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async virtual Task<List<Models.LearningProgression>> GetLearningProgressionsForCustomerAsync(Guid customerId)
        {
            return await _documentDbProvider.GetLearningProgressionsForCustomerAsync(customerId);
        }
    }
}
