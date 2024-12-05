using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.LearningProgression.Models;
using System.Text.Json;

namespace NCS.DSS.LearningProgression.Cosmos.Provider
{
    public class CosmosDBProvider : ICosmosDBProvider
    {
        private readonly Container _learningProgressionContainer;
        private readonly Container _customerContainer;
        private readonly PartitionKey _partitionKey = PartitionKey.None;
        private readonly ILogger<CosmosDBProvider> _logger;

        public CosmosDBProvider(
            CosmosClient cosmosClient, 
            IOptions<LearningProgressionConfigurationSettings> configOptions,
            ILogger<CosmosDBProvider> logger)
        {
            var config = configOptions.Value;

            _learningProgressionContainer = GetContainer(cosmosClient, config.DatabaseId, config.CollectionId);
            _customerContainer = GetContainer(cosmosClient, config.CustomerDatabaseId, config.CustomerCollectionId);
            _logger = logger;
        }

        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId) 
            => cosmosClient.GetContainer(databaseId, collectionId);

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            try
            {
                var response = await _customerContainer.ReadItemAsync<Customer>(
                    customerId.ToString(),
                    _partitionKey);

                return response.Resource != null;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If a 404 occurs, the resource does not exist
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking customer resource existence. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            _logger.LogInformation("Checking for termination date. Customer ID: {CustomerId}", customerId);

            try
            {
                var response = await _customerContainer.ReadItemAsync<Customer>(
                    customerId.ToString(),
                    _partitionKey);

                var dateOfTermination = response.Resource?.DateOfTermination;
                var hasTerminationDate = dateOfTermination != null;

                _logger.LogInformation("Termination date check completed. CustomerId: {CustomerId}. HasTerminationDate: {HasTerminationDate}", customerId, hasTerminationDate);
                return hasTerminationDate;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If a 404 occurs, the resource does not exist
                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking termination date. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> DoesLearningProgressionExistForCustomer(Guid customerId)
        {
            try
            {
                var query = _learningProgressionContainer.GetItemLinqQueryable<Models.LearningProgression>()
                    .Where(x => x.CustomerId == customerId)
                    .Take(1)
                    .ToFeedIterator();

                var response = await query.ReadNextAsync();
                return response.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking LearningProgression existence. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<Models.LearningProgression?> GetLearningProgressionForCustomerAsync(Guid customerId, Guid learningProgressionId)
        {
            _logger.LogInformation("Retrieving LearningProgression. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);

            try
            {
                var query = _learningProgressionContainer.GetItemLinqQueryable<Models.LearningProgression>()
                    .Where(x => x.CustomerId == customerId && x.LearningProgressionId == learningProgressionId)
                    .ToFeedIterator();

                var response = await query.ReadNextAsync();
                if (response.Any())
                {
                    _logger.LogInformation("Successfully retrieved LearningProgression. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);
                    return response.FirstOrDefault();
                }

                _logger.LogWarning("No LearningProgression found. Customer ID: {CustomerId}, Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving LearningProgression. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);
                throw;
            }
        }

        public async Task<List<Models.LearningProgression>> GetLearningProgressionsForCustomerAsync(Guid customerId)
        {
            try
            {
                var query = _learningProgressionContainer.GetItemLinqQueryable<Models.LearningProgression>()
                    .Where(x => x.CustomerId == customerId)
                    .ToFeedIterator();

                var learningProgressions = new List<Models.LearningProgression>();
                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    learningProgressions.AddRange(response);
                }

                _logger.LogInformation("Found {Count} LearningProgression(s). Customer ID: {CustomerId}", learningProgressions.Count, customerId);
                return learningProgressions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving LearningProgressions for customer. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<ItemResponse<Models.LearningProgression>> CreateLearningProgressionAsync(Models.LearningProgression learningProgression)
        {
            _logger.LogInformation("Creating LearningProgression. Customer ID: {CustomerId}", learningProgression.CustomerId);

            var response = await _learningProgressionContainer.CreateItemAsync(
                learningProgression,
                _partitionKey);

            _logger.LogInformation("Finished creating LearningProgression. Customer ID: {CustomerId}", learningProgression.CustomerId);
            return response;
        }

        //TODO: Update codebase to require learningProgression objects to avoid multiple deserializing and serializing
        public async Task<ItemResponse<Models.LearningProgression>?> UpdateLearningProgressionAsync(string learningProgressionJson, Guid learningProgressionId)
        {
            _logger.LogInformation("Updating LearningProgression. Learning Progression ID: {LearningProgressionId}", learningProgressionId);

            if (string.IsNullOrEmpty(learningProgressionJson))
            {
                _logger.LogWarning("NULL or empty JSON provided for {FunctionName}. Learning Progression ID: {LearningProgressionId}", nameof(UpdateLearningProgressionAsync), learningProgressionId);
                return null;
            }

            try
            {
                var learningProgression = JsonSerializer.Deserialize<Models.LearningProgression>(learningProgressionJson);
                if (learningProgression == null)
                {
                    _logger.LogWarning("Deserialization failed for LearningProgression JSON. Learning Progression ID: {LearningProgressionId}", learningProgressionId);
                    return null;
                }

                if (learningProgression.LearningProgressionId != learningProgressionId)
                {
                    _logger.LogWarning("Mismatch between provided ID and document ID. Provided ID: {LearningProgressionId}, Document ID: {DocumentId}",
                        learningProgressionId, learningProgression.LearningProgressionId);
                    return null;
                }

                var response = await _learningProgressionContainer.ReplaceItemAsync(learningProgression,
                    learningProgressionId.ToString(),
                    _partitionKey);

                _logger.LogInformation("LearningProgression updated successfully. Learning Progression ID: {LearningProgressionId}", learningProgressionId);
                return response;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If a 404 occurs, the resource does not exist
                _logger.LogWarning("LearningProgression not found during update. Learning Progression ID: {LearningProgressionId}", learningProgressionId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating LearningProgression. Learning Progression ID: {LearningProgressionId}", learningProgressionId);
                throw;
            }
        }

        public async Task<ItemResponse<Models.LearningProgression>> UpdateLearningProgressionAsync(Models.LearningProgression learningProgression)
        {
            _logger.LogInformation("Updating LearningProgression. Learning Progression ID: {LearningProgressionId}", learningProgression.LearningProgressionId);

            var response = await _learningProgressionContainer.ReplaceItemAsync(
                learningProgression,
                learningProgression.LearningProgressionId.ToString(),
                _partitionKey);

            _logger.LogInformation("LearningProgression updated successfully. Learning Progression ID: {LearningProgressionId}", learningProgression.LearningProgressionId);
            return response;
        }

        public async Task<string?> GetLearningProgressionForCustomerToPatchAsync(Guid customerId, Guid learningProgressionId)
        {
            _logger.LogInformation("Attempting to retrieve LearningProgression for PATCH request. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);

            try
            {
                var query = _learningProgressionContainer.GetItemLinqQueryable<Models.LearningProgression>()
                    .Where(x => x.CustomerId == customerId && x.LearningProgressionId == learningProgressionId)
                    .Take(1)
                    .ToFeedIterator();

                var response = await query.ReadNextAsync();
                var learningProgression = response.FirstOrDefault();

                if (learningProgression != null)
                {
                    _logger.LogInformation("Retrieved LearningProgression for PATCH request. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);
                    return JsonSerializer.Serialize(learningProgression);
                }

                _logger.LogWarning("No LearningProgression available for PATCH request. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving LearningProgression for PATCH request. Customer ID: {CustomerId}. Learning Progression ID: {LearningProgressionId}", customerId, learningProgressionId);
                throw;
            }
        }
    }
}
