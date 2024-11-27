namespace NCS.DSS.LearningProgression.Models
{
    public class LearningProgressionConfigurationSettings
    {
        public string CosmosDBConnectionString { get; set; }
        public string QueueName { get; internal set; }
        public string ServiceBusConnectionString { get; set; }
        public string DatabaseId { get; set; }
        public string CollectionId { get; set; }
        public string CustomerDatabaseId { get; set; }
        public string CustomerCollectionId { get; set; }
    }
}