using NCS.DSS.LearningProgression.Cosmos.Provider;

namespace NCS.DSS.LearningProgression.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        private readonly ICosmosDBProvider _documentDbProvider;

        public ResourceHelper(ICosmosDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            var doesCustomerExist = await _documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            var isCustomerReadOnly = await _documentDbProvider.DoesCustomerHaveATerminationDate(customerId);

            return isCustomerReadOnly;
        }
    }
}
