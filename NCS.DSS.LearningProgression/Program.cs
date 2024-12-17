using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.LearningProgression.Cosmos.Helper;
using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.GetLearningProgression.Service;
using NCS.DSS.LearningProgression.GetLearningProgressionById.Service;
using NCS.DSS.LearningProgression.Models;
using NCS.DSS.LearningProgression.PatchLearningProgression.Service;
using NCS.DSS.LearningProgression.PostLearningProgression.Service;
using NCS.DSS.LearningProgression.ServiceBus;
using NCS.DSS.LearningProgression.Validators;

namespace NCS.DSS.LearningProgression
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.SetBasePath(Environment.CurrentDirectory)
                        .AddJsonFile("local.settings.json", optional: true,
                            reloadOnChange: false)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    services.AddOptions<LearningProgressionConfigurationSettings>()
                        .Bind(configuration);

                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddLogging();
                    services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
                    services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
                    services.AddTransient<IJsonHelper, JsonHelper>();
                    services.AddTransient<IServiceBusClient, ServiceBusClient>();
                    services.AddTransient<ICosmosDBProvider, CosmosDBProvider>();
                    services.AddTransient<IResourceHelper, ResourceHelper>();
                    services.AddTransient<IValidate, Validate>();
                    services.AddSingleton<IDynamicHelper, DynamicHelper>();
                    services.AddTransient<ILearningProgressionsGetTriggerService, LearningProgressionsGetTriggerService>();
                    services.AddTransient<ILearningProgressionGetByIdService, LearningProgressionGetByIdService>();
                    services.AddTransient<ILearningProgressionPatchTriggerService, LearningProgressionPatchTriggerService>();
                    services.AddTransient<ILearningProgressionPostTriggerService, LearningProgressionPostTriggerService>();

                    services.AddSingleton(sp =>
                    {
                        var settings = sp.GetRequiredService<IOptions<LearningProgressionConfigurationSettings>>().Value;
                        var options = new CosmosClientOptions()
                        {
                            ConnectionMode = ConnectionMode.Gateway
                        };

                        return new CosmosClient(settings.CosmosDBConnectionString, options);
                    });

                    services.AddSingleton(serviceProvider =>
                    {
                        var settings = serviceProvider.GetRequiredService<IOptions<LearningProgressionConfigurationSettings>>().Value;
                        return new Azure.Messaging.ServiceBus.ServiceBusClient(settings.ServiceBusConnectionString);
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.Services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule? defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (defaultRule is not null)
                        {
                            options.Rules.Remove(defaultRule);
                        }
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}