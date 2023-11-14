using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.CosmosDocumentClient;
using NCS.DSS.LearningProgression.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.LearningProgression.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly ICosmosDocumentClient _cosmosDocumentClient;
        private ILogger<DocumentDBProvider> _logger;

        public DocumentDBProvider(ICosmosDocumentClient cosmosDocumentClient, ILogger<DocumentDBProvider> logger)
        {
            _cosmosDocumentClient = cosmosDocumentClient;
            _logger = logger;
        }

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            _logger.LogInformation($"Started checking for Resourse Existance for CustomerID [{customerId}]");

            var documentUri = DocumentDBUrlHelper.CreateCustomerDocumentUri(customerId);
            bool isExists;
            try
            {
                var client = _cosmosDocumentClient.GetDocumentClient();
                var response = await client.ReadDocumentAsync(documentUri);
                isExists = response.Resource != null;

            }
            catch (DocumentClientException ex)
            {    
                isExists = false;
                _logger.LogError($"Document Client Exception Raised for [{customerId}]. Exception Message [{ex.Message}]. Stacktrace [{ex.StackTrace}]");
            }

            if (isExists)
            {
                _logger.LogInformation($"Resourse Exists for CustomerID [{customerId}]");
            }
            else
            {
                _logger.LogWarning($"Resourse Does Exists for CustomerID [{customerId}]");
            }
            
            return isExists;
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            _logger.LogInformation($"Started checking for Termination Date of CustomerID [{customerId}]");
            var documentUri = DocumentDBUrlHelper.CreateCustomerDocumentUri(customerId);

            bool isActive;
            try
            {
                var client = _cosmosDocumentClient.GetDocumentClient();
                var response = await client.ReadDocumentAsync(documentUri);
                var dateOfTermination = response.Resource?.GetPropertyValue<DateTime?>("DateOfTermination");
                isActive = dateOfTermination.HasValue;
            }
            catch (DocumentClientException ex)
            {
                isActive = false;
                _logger.LogError($"Document Client Exception Raised for [{customerId}]. Exception Message [{ex.Message}]. Stacktrace [{ex.StackTrace}]");
            }

            if (isActive)
            {
                _logger.LogInformation($"Termination Date exists for CustomerID [{customerId}]");
            }
            else
            {
                _logger.LogWarning($"Termination Date not exists for CustomerID [{customerId}]");
            }

            return isActive;
        }

        public bool DoesLearningProgressionExistForCustomer(Guid customerId)
        {
            _logger.LogInformation($"Started checking for Learning Progression of CustomerID [{customerId}]");
            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            {
                _logger.LogError($"Failed to get the Document Client while chekcing for Learning Progression of CustomerID [{customerId}]");
                return false;
            }

            var learningProgressionForCustomerQuery = client.CreateDocumentQuery<Models.LearningProgression>(collectionUri, new FeedOptions { MaxItemCount = 1 });
            var result = learningProgressionForCustomerQuery.Where(x => x.CustomerId == customerId).AsEnumerable().Any();

            if (result)
            {
                _logger.LogInformation($"Learning Progression exists for CustomerID [{customerId}]");
            }
            else
            {
                _logger.LogWarning($"Learning Progression not exists for CustomerID [{customerId}]");
            }
            return result;
        }

        public async Task<Models.LearningProgression> GetLearningProgressionForCustomerAsync(Guid customerId, Guid learningProgressionId)
        {
            _logger.LogInformation($"Started retrieving details of Learning Progression of CustomerID [{customerId}] with Learning Progression ID [{learningProgressionId}]");

            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            var learningProgressionForCustomerQuery = client
                ?.CreateDocumentQuery<Models.LearningProgression>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.LearningProgressionId == learningProgressionId)
                .AsDocumentQuery();

            if (learningProgressionForCustomerQuery == null)
            {
                _logger.LogWarning($"Can't find the Learning Progression of CustomerID [{customerId}] with Learning Progression ID [{learningProgressionId}]");

                return null;
            }

            var learningProgression = await learningProgressionForCustomerQuery.ExecuteNextAsync<Models.LearningProgression>();
            if(learningProgression.Count > 0)
            {
                _logger.LogInformation($"Successfully retrieved learning progression of CustomerID [{customerId}] with Learning Progression ID [{learningProgressionId}]");
            }
            else
                _logger.LogWarning($"No Learning Progression found with CustomerID [{customerId}] and Learning Progression ID [{learningProgressionId}]");

            return learningProgression?.FirstOrDefault();
        }

        public async Task<List<Models.LearningProgression>> GetLearningProgressionsForCustomerAsync(Guid customerId)
        {
            _logger.LogInformation($"Started retrieving list of Learning Progressions of a CustomerID [{customerId}]");

            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();

            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            {
                _logger.LogError($"Failed to get the Document Client while retrieving lisf of Learning Progressions of a CustomerID [{customerId}]");
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
                _logger.LogInformation($"Found [{learningProgressions.Count}] Learning Progressions for Customer with ID [{customerId}]");
                return learningProgressions;
            }
            else
            {
                _logger.LogWarning($"No Learning Progressions found for Customer with ID [{customerId}]");
                return null;
            }
                
        }

        public async Task<ResourceResponse<Document>> CreateLearningProgressionAsync(Models.LearningProgression learningProgression)
        {
            _logger.LogInformation($"Started Creating Learning Progressions of a CustomerID [{learningProgression.CustomerId}] in Cosmos DB");
            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();

            var client = _cosmosDocumentClient.GetDocumentClient();
            var response = await client.CreateDocumentAsync(collectionUri, learningProgression);

            _logger.LogInformation($"Successfully Created Learning Progressions of a CustomerID [{learningProgression.CustomerId}] in Cosmos DB");

            return response;
        }

        public async Task<ResourceResponse<Document>> UpdateLearningProgressionAsync(string learningProgressionJson, Guid learningProgressionId)
        {
            _logger.LogInformation($"Started Updating Learning Progressions with ID [{learningProgressionId}] in Cosmos DB");
            if (string.IsNullOrEmpty(learningProgressionJson))
            {
                _logger.LogWarning($"Invalid Json. Can't Progress on Updating Learning Progressions with ID [{learningProgressionId}] in Cosmos DB");
                return null;
            }

            var documentUri = DocumentDBUrlHelper.CreateDocumentUri(learningProgressionId);
            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            {
                _logger.LogError($"Failed to get the Document Client while Updating Learning Progressions with ID [{learningProgressionId}]");
                return null;
            }

            var learningProgressionDocumentJObject = JObject.Parse(learningProgressionJson);
            var response = await client.ReplaceDocumentAsync(documentUri, learningProgressionDocumentJObject);

            _logger.LogInformation($"Successfully Updated Learning Progression with ID [{learningProgressionId}] in Cosmos DB");

            return response;
        }

        public async Task<string> GetLearningProgressionForCustomerToPatchAsync(Guid customerId, Guid learningProgressionId)
        {
            _logger.LogInformation($"Started Getting Learning Progression with ID [{learningProgressionId}] of the customer with ID [{customerId}] in Cosmos DB");

            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            var learningProgressionQuery = client
                ?.CreateDocumentQuery<Models.LearningProgression>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.LearningProgressionId == learningProgressionId)
                .AsDocumentQuery();

            if (learningProgressionQuery == null)
            {
                _logger.LogError($"Failed to get the Document Client while retrieving Learning Progressions with ID [{learningProgressionId}] of the customer with ID [{customerId}] ");
                return null;
            }

            var learningProgressions = await learningProgressionQuery.ExecuteNextAsync();

            _logger.LogInformation($"Successfully retrieved Learning Progression with ID [{learningProgressionId}] of the customer with ID [{customerId}] in Cosmos DB");

            return learningProgressions?.FirstOrDefault()?.ToString();
        }
    }
}