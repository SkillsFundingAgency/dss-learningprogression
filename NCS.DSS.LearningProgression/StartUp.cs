using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using DFC.Swagger.Standard;
using NCS.DSS.LearningProgression;
using System;
using DFC.HTTP.Standard;
using NCS.DSS.LearningProgression.ServiceBus;
using NCS.DSS.LearningProgression.Services;
using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.CosmosDocumentClient;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.Validators;

[assembly: FunctionsStartup(typeof(Startup))]

namespace NCS.DSS.LearningProgression
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var settings = GetLearnerProgressConfigurationSettings();

            services.AddSingleton(settings);
            services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            services.AddTransient<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
            services.AddTransient<IServiceBusClient, ServiceBusClient>();
            services.AddTransient<ILearningProgressionServices, LearningProgressionServices>();
            services.AddTransient<IDocumentDBProvider, DocumentDBProvider>();
            services.AddTransient<IJsonHelper, JsonHelper>();
            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IValidate, Validate>();

            services.AddSingleton<ICosmosDocumentClient, CosmosDocumentClient.CosmosDocumentClient>(x => new CosmosDocumentClient.CosmosDocumentClient(settings.CosmosDBConnectionString));
        }

        private LearnerProgressConfigurationSettings GetLearnerProgressConfigurationSettings()
        {
            var settings = new LearnerProgressConfigurationSettings
            {
                CosmosDBConnectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString"),
                KeyName = Environment.GetEnvironmentVariable("KeyName"),
                AccessKey = Environment.GetEnvironmentVariable("AccessKey"),
                BaseAddress = Environment.GetEnvironmentVariable("BaseAddress"),
                QueueName = Environment.GetEnvironmentVariable("QueueName"),
            };

            return settings;
        }
    }
}
