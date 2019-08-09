using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.LearningProgression.CosmosDocumentClient
{
    public class CosmosDocumentClient : ICosmosDocumentClient
    {
        const string AccountEndpointId = "AccountEndpoint=";
        const string AccountKeyId = "AccountKey=";

        private readonly IDocumentClient _documentClient;

        public CosmosDocumentClient(string connectionString)
        {
            try
            {
                var endpoint = GetEndpoint(connectionString);
                var accountKey = GetAccountKey(connectionString);

                _documentClient = new DocumentClient(new Uri(endpoint), accountKey);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IDocumentClient GetDocumentClient()
        {
            return _documentClient;
        }

        private string GetAccountKey(string connectionString)
        {
            return connectionString.Split(new[] { AccountKeyId }, StringSplitOptions.None)[1]
                .Split(';')[0]
                .Trim();
        }

        private string GetEndpoint(string connectionString)
        {
            return connectionString.Split(new[] { AccountEndpointId }, StringSplitOptions.None)[1]
                .Split(';')[0]
                .Trim();
        }
    }
}
