using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.CosmosDocumentClient;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.LearningProgression.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly ICosmosDocumentClient _cosmosDocumentClient;

        public DocumentDBProvider(ICosmosDocumentClient cosmosDocumentClient)
        {
            _cosmosDocumentClient = cosmosDocumentClient;
        }

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            var documentUri = DocumentDBUrlHelper.CreateCustomerDocumentUri(customerId);

            try
            {
                var client = _cosmosDocumentClient.GetDocumentClient();
                var response = await client.ReadDocumentAsync(documentUri);
                if (response.Resource != null)
                {
                    return true;
                }
            }
            catch (DocumentClientException)
            {
                return false;
            }

            return false;
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var documentUri = DocumentDBUrlHelper.CreateCustomerDocumentUri(customerId);

            try
            {
                var client = _cosmosDocumentClient.GetDocumentClient();
                var response = await client.ReadDocumentAsync(documentUri);
                var dateOfTermination = response.Resource?.GetPropertyValue<DateTime?>("DateOfTermination");

                return dateOfTermination.HasValue;
            }
            catch (DocumentClientException)
            {
                return false;
            }
        }

        public bool DoesLearningProgressionExistForCustomer(Guid customerId)
        {
            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            {
                return false;
            }

            var learningProgressionForCustomerQuery = client.CreateDocumentQuery<Models.LearningProgression>(collectionUri, new FeedOptions { MaxItemCount = 1 });
            var result = learningProgressionForCustomerQuery.Where(x => x.CustomerId == customerId).AsEnumerable().Any();

            return result;
        }

        public async Task<Models.LearningProgression> GetLearningProgressionForCustomerAsync(Guid customerId, Guid learningProgressionId)
        {
            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            var learningProgressionForCustomerQuery = client
                ?.CreateDocumentQuery<Models.LearningProgression>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.LearningProgressionId == learningProgressionId)
                .AsDocumentQuery();

            if (learningProgressionForCustomerQuery == null)
            {
                return null;
            }

            var learningProgression = await learningProgressionForCustomerQuery.ExecuteNextAsync<Models.LearningProgression>();

            return learningProgression?.FirstOrDefault();
        }

        public async Task<List<Models.LearningProgression>> GetLearningProgressionsForCustomerAsync(Guid customerId)
        {
            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();

            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            {
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

            return learningProgressions.Any() ? learningProgressions : null;
        }

        public async Task<ResourceResponse<Document>> CreateLearningProgressionAsync(Models.LearningProgression learningProgression)
        {
            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();

            var client = _cosmosDocumentClient.GetDocumentClient();
            var response = await client.CreateDocumentAsync(collectionUri, learningProgression);

            return response;
        }

        public async Task<ResourceResponse<Document>> UpdateLearningProgressionAsync(string learningProgressionJson, Guid learningProgressionId)
        {
            if (string.IsNullOrEmpty(learningProgressionJson))
            {
                return null;
            }

            var documentUri = DocumentDBUrlHelper.CreateDocumentUri(learningProgressionId);
            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            {
                // todo Log fact client is null when it should have a value
                return null;
            }

            var learningProgressionDocumentJObject = JObject.Parse(learningProgressionJson);
            var response = await client.ReplaceDocumentAsync(documentUri, learningProgressionDocumentJObject);

            return response;
        }

        public async Task<string> GetLearningProgressionForCustomerToPatchAsync(Guid customerId, Guid learningProgressionId)
        {
            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            var learningProgressionQuery = client
                ?.CreateDocumentQuery<Models.LearningProgression>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.LearningProgressionId == learningProgressionId)
                .AsDocumentQuery();

            if (learningProgressionQuery == null)
            {
                return null;
            }

            var learningProgressions = await learningProgressionQuery.ExecuteNextAsync();
            return learningProgressions?.FirstOrDefault()?.ToString();
        }
    }
}