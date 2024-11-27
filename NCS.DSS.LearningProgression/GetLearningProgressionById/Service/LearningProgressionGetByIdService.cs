using NCS.DSS.LearningProgression.Cosmos.Provider;

namespace NCS.DSS.LearningProgression.GetLearningProgressionById.Service
{
    public class LearningProgressionGetByIdService : ILearningProgressionGetByIdService
    {
        private readonly ICosmosDBProvider _documentDbProvider;

        public LearningProgressionGetByIdService(ICosmosDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public virtual async Task<Models.LearningProgression> GetLearningProgressionForCustomerAsync(Guid customerId, Guid progressionProgressionId)
        {
            return await _documentDbProvider.GetLearningProgressionForCustomerAsync(customerId, progressionProgressionId);
        }
    }
}
