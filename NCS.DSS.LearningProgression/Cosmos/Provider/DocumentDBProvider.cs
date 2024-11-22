using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.Cosmos.Client;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.LearningProgression.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly ICosmosDocumentClient _cosmosDocumentClient;
        private readonly ILogger<DocumentDBProvider> _logger;

        public DocumentDBProvider(ICosmosDocumentClient cosmosDocumentClient, ILogger<DocumentDBProvider> logger)
        {
            _cosmosDocumentClient = cosmosDocumentClient;
            _logger = logger;
        }

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            _logger.LogInformation("Checking if customer resource exists. Customer ID: {CustomerId}", customerId);

            var documentUri = DocumentDBUrlHelper.CreateCustomerDocumentUri(customerId);
            var client = _cosmosDocumentClient.GetDocumentClient();

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);
                var exists = response.Resource != null;
                _logger.LogInformation("Customer resource existence check completed. Customer ID: {CustomerId}. Exists: {Exists}", customerId, exists);
                return exists;
            }
            catch (DocumentClientException ex)
            {
                _logger.LogError(ex, "Error checking customer resource existence. Customer ID: {CustomerId}", customerId);
                return false;
            }
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            _logger.LogInformation("Checking for termination date. Customer ID: {CustomerId}", customerId);

            var documentUri = DocumentDBUrlHelper.CreateCustomerDocumentUri(customerId);
            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            {
                _logger.LogError("Failed to get Document Client while checking termination date. Customer ID: {CustomerId}", customerId);
                return false;
            }

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);
                var dateOfTermination = response.Resource?.GetPropertyValue<DateTime?>("DateOfTermination");
                var hasTerminationDate = dateOfTermination.HasValue;

                _logger.LogInformation("Termination date check completed. CustomerId: {CustomerId}. HasTerminationDate: {HasTerminationDate}", customerId, hasTerminationDate);
                return hasTerminationDate;
            }
            catch (DocumentClientException ex)
            {
                _logger.LogError(ex, "Error checking termination date. Customer ID: {CustomerId}", customerId);
                return false;
            }
        }

        public bool DoesLearningProgressionExistForCustomer(Guid customerId)
        {
            _logger.LogInformation("Checking if LearningProgression exists. Customer ID: {CustomerId}", customerId);

            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            {
                _logger.LogError("Failed to get Document Client while checking LearningProgression existence. Customer ID: {CustomerId}", customerId);
                return false;
            }

            var learningProgressionForCustomerQuery = client.CreateDocumentQuery<Models.LearningProgression>(collectionUri,
                new FeedOptions { MaxItemCount = 1 });
            var result = learningProgressionForCustomerQuery
                .Where(x => x.CustomerId == customerId)
                .AsEnumerable()
                .Any();

            _logger.LogInformation("LearningProgression existence check completed. Customer ID: {CustomerId}. Exists: {Exists}", customerId, result);
            return result;
        }

        public async Task<Models.LearningProgression> GetLearningProgressionForCustomerAsync(Guid customerId, Guid learningProgressionId)
        {
            _logger.LogInformation("Retrieving LearningProgression. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);

            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            var learningProgressionForCustomerQuery = client
                ?.CreateDocumentQuery<Models.LearningProgression>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.LearningProgressionId == learningProgressionId)
                .AsDocumentQuery();

            if (learningProgressionForCustomerQuery == null)
            {
                _logger.LogWarning("LearningProgression query could not be created. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);
                return null;
            }

            var learningProgression = await learningProgressionForCustomerQuery.ExecuteNextAsync<Models.LearningProgression>();
            if (learningProgression.Count > 0)
            {
                _logger.LogInformation("Successfully retrieved LearningProgression. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);
            }
            else
            {
                _logger.LogWarning("No LearningProgression found. Customer ID: {CustomerId}, Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);
            }

            return learningProgression?.FirstOrDefault();
        }

        public async Task<List<Models.LearningProgression>> GetLearningProgressionsForCustomerAsync(Guid customerId)
        {
            _logger.LogInformation("Retrieving LearningProgressions for customer. Customer ID: {CustomerId}", customerId);

            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            {
                _logger.LogError("Failed to get Document Client while retrieving LearningProgressions. Customer ID: {CustomerId}", customerId);
                return null;
            }

            var learningProgressionsQuery = client.CreateDocumentQuery<Models.LearningProgression>(collectionUri)
                .Where(so => so.CustomerId == customerId).AsDocumentQuery();

            var learningProgressions = new List<Models.LearningProgression>();

            while (learningProgressionsQuery.HasMoreResults)
            {
                var response = await learningProgressionsQuery.ExecuteNextAsync<Models.LearningProgression>();
                learningProgressions.AddRange(response);
            }

            if (learningProgressions.Any())
            {
                _logger.LogInformation("LearningProgressions retrieved. Customer ID: {CustomerId}, Count: {Count}", customerId, learningProgressions.Count);
                return learningProgressions;
            }

            _logger.LogWarning("No LearningProgressions found for customer. Customer ID: {CustomerId}", customerId);
            return null;
        }

        public async Task<ResourceResponse<Document>> CreateLearningProgressionAsync(Models.LearningProgression learningProgression)
        {
            _logger.LogInformation("Creating LearningProgression. Customer ID: {CustomerId}", learningProgression.CustomerId);

            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();
            var response = await client.CreateDocumentAsync(collectionUri, learningProgression);

            _logger.LogInformation("LearningProgression created successfully. Customer ID: {CustomerId}, Learning Progression ID: {LearningProgressionId}", learningProgression.CustomerId, learningProgression.LearningProgressionId);

            return response;
        }

        public async Task<ResourceResponse<Document>> UpdateLearningProgressionAsync(string learningProgressionJson, Guid learningProgressionId)
        {
            _logger.LogInformation("Updating LearningProgression. Learning Progression ID: {LearningProgressionId}", learningProgressionId);

            if (string.IsNullOrEmpty(learningProgressionJson))
            {
                _logger.LogWarning("NULL or empty JSON provided for {FunctionName}. Learning Progression ID: {LearningProgressionId}", nameof(UpdateLearningProgressionAsync), learningProgressionId);
                return null;
            }

            var documentUri = DocumentDBUrlHelper.CreateDocumentUri(learningProgressionId);
            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            {
                _logger.LogError("Failed to get Document Client while updating LearningProgression. Learning Progression ID: {LearningProgressionId}", learningProgressionId);
                return null;
            }

            var learningProgressionDocumentJObject = JObject.Parse(learningProgressionJson);
            var response = await client.ReplaceDocumentAsync(documentUri, learningProgressionDocumentJObject);

            _logger.LogInformation("LearningProgression updated successfully. Customer ID: {LearningProgressionId}", learningProgressionId);

            return response;
        }

        public async Task<string> GetLearningProgressionForCustomerToPatchAsync(Guid customerId, Guid learningProgressionId)
        {
            _logger.LogInformation("Retrieving LearningProgression for PATCH. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);

            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            var learningProgressionQuery = client
                ?.CreateDocumentQuery<Models.LearningProgression>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.LearningProgressionId == learningProgressionId)
                .AsDocumentQuery();

            if (learningProgressionQuery == null)
            {
                _logger.LogWarning("No LearningProgression found for PATCH. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);
                return null;
            }

            var learningProgression = await learningProgressionQuery.ExecuteNextAsync();
            if (learningProgression.Count > 0)
            {
                _logger.LogInformation("LearningProgression retrieved for PATCH. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);
                return learningProgression.FirstOrDefault()?.ToString();
            }

            _logger.LogWarning("No LearningProgression available for patch. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);
            return null;
        }
    }
}
