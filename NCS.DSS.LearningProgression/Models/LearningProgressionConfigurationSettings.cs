namespace NCS.DSS.LearningProgression.Models
{
    public class LearningProgressionConfigurationSettings
    {
        public required string CosmosDBConnectionString { get; set; }
        public required string QueueName { get; set; }
        public required string ServiceBusConnectionString { get; set; }
        public required string DatabaseId { get; set; }
        public required string CollectionId { get; set; }
        public required string CustomerDatabaseId { get; set; }
        public required string CustomerCollectionId { get; set; }
    }
}