namespace NCS.DSS.LearningProgression.Models
{
    public class LearningProgressionConfigurationSettings
    {
        public string CosmosDBConnectionString { get; set; }
        public string QueueName { get; internal set; }
        public string ServiceBusConnectionString { get; set; }
    }
}