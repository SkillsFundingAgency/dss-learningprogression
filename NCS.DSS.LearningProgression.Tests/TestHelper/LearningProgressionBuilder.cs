using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.Cosmos.Provider;
using NCS.DSS.LearningProgression.CosmosDocumentClient;
using NCS.DSS.LearningProgression.ServiceBus;
using NCS.DSS.LearningProgression.Services;
using NCS.DSS.LearningProgression.Validators;
//using NCS.DSS.LearningProgression.Cosmos.Provider;
//using NCS.DSS.LearningProgression.CosmosDocumentClient;
//using NCS.DSS.LearningProgression.ServiceBus;
//using NCS.DSS.LearningProgression.Services;
//using NCS.DSS.LearningProgression.Validators;
using NSubstitute;
using System;
using System.Collections.Generic;

namespace NCS.DSS.LearningProgression.Tests.TestHelper
{
    public class LearningProgressionBuilder
    {
        public Dictionary<string, StringValues> Headers { get; set; }
        public Dictionary<string, StringValues> Query { get; set; }
        public string Body { get; set; }
        public ILogger Logger { get; set; }
        public DefaultHttpRequest Request { get; set; }
        public IValidate Validate { get; set; }
        public string ConnectionString { get; set; }
        public string CustomerId { get; set; }
        public IHttpResponseMessageHelper ResponseMessageHelper { get; set; }
        public IHttpRequestHelper RequestHelper { get; set; }
        public ILearningProgressionServices LearningProgressionServices { get; set; }
        public IServiceBusClient ServiceBusClient { get; set; }
        public IDocumentDBProvider DocumentDBProvider { get; set; }

        public LearnerProgressConfigurationSettings LearnerProgressConfigurationSettings { get; set; }
        public IJsonHelper JsonHelper { get; internal set; }
        public IResourceHelper ResourceHelper { get; internal set; }
        public ICosmosDocumentClient CosmosDBClient { get; internal set; }

        public LearningProgressionBuilder(string connectionString)
        {
            Request = new DefaultHttpRequest(new DefaultHttpContext());
            ConnectionString = connectionString;
        }

        public LearningProgressionBuilder WithLearningProgressionServiceWithCustomerExist()
        {
            LearningProgressionServices = Substitute.For<LearningProgressionServices>(DocumentDBProvider, ServiceBusClient, LearnerProgressConfigurationSettings, Logger, JsonHelper);
            LearningProgressionServices.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            return this;
        }

        public LearningProgressionBuilder WithLearningProgressionServiceWithCustomerNotExist()
        {
            LearningProgressionServices = Substitute.For<LearningProgressionServices>(DocumentDBProvider, ServiceBusClient, LearnerProgressConfigurationSettings, Logger, JsonHelper);
            LearningProgressionServices.DoesCustomerExist(Guid.NewGuid()).Returns(false);
            return this;
        }

        public LearningProgressionBuilder WithResponseMessageHelper()
        {
            ResponseMessageHelper = new HttpResponseMessageHelper();
            return this;
        }

        public LearningProgressionBuilder WithRequestHelper()
        {
            RequestHelper = new HttpRequestHelper();
            return this;
        }

        public LearningProgressionBuilder WithCustomerId(string customerId)
        {
            CustomerId = customerId;
            return this;
        }

        public LearningProgressionBuilder WithValidator()
        {
            Validate = new Validate();
            return this;
        }

        public LearningProgressionBuilder WithSubstituteValidator()
        {
            Validate = Substitute.For<IValidate>();
            return this;
        }

        public LearningProgressionBuilder WithQuery()
        {
            Request.Query = new QueryCollection(Query);

            return this;
        }

        public LearningProgressionBuilder WithHeader(Dictionary<string, StringValues> headers)
        {
            foreach (var header in headers)
            {
                Headers.Add(header.Key, header.Value);
            }

            return this;
        }

        public LearningProgressionBuilder WithBody(string body)
        {

            return this;
        }

        public LearningProgressionBuilder WithLogger()
        {
            Logger = new ListLogger();

            return this;
        }

        public LearningProgressionBuilder WithNullLogger()
        {
            Logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");

            return this;
        }

        public LearningProgressionBuilder WithJsonHelper()
        {
            JsonHelper = new JsonHelper();

            return this;
        }

        public LearningProgressionBuilder WithCosmosDBClient()
        {
            CosmosDBClient = new CosmosDocumentClient.CosmosDocumentClient(ConnectionString);

            return this;
        }

        public LearningProgressionBuilder WithResourceHelper()
        {
            ResourceHelper = new ResourceHelper(DocumentDBProvider);

            return this;
        }

        public DefaultHttpRequest Build()
        {
            return Request;
        }
    }
}
