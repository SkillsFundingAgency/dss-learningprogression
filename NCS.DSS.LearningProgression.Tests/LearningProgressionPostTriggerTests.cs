using Microsoft.Extensions.Primitives;
using NCS.DSS.LearningProgression.Tests.TestHelper;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Net;

namespace NCS.DSS.LearningProgression.Tests
{
    public class LearningProgressionPostTriggerTests
    {
        public LearningProgressionPostTriggerTests()
        {
        }

        [Fact]
        async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            // arrange
            var headers = new Dictionary<string, StringValues>();
            var body = string.Empty;
            var builder = new LearningProgressionBuilder("AccountEndpoint=https://dss-at-shared-cdb.documents.azure.com:443/;AccountKey=gN8fmksIp9YAz7a3GHARnMlr7EdA3MWg3vEVEHkIrWfqHhl9Mpxd651fwmgqfCEgmuFEaGDU3OJLNouL6zlbmQ==;");

            builder.WithHeader(headers)
            .WithBody(body)
            .WithNullLogger()
            .WithSubstituteValidator()
            .WithValidator()
            .WithCustomerId("1")
            .WithRequestHelper()
            .WithResponseMessageHelper()
            .WithJsonHelper()
            .WithCosmosDBClient()
            .WithResourceHelper()
            .WithLearningProgressionServiceWithCustomerExist();

            var httpPostFunction = new LearningProgressionPostTrigger(
                builder.LearnerProgressConfigurationSettings,
                builder.ResponseMessageHelper,
                builder.RequestHelper,
                builder.LearningProgressionServices,
                builder.JsonHelper,
                builder.ResourceHelper,
                builder.CosmosDBClient,
                builder.Validate
                );

            // Act
            var response = await httpPostFunction.Run(builder.Request, builder.Logger, builder.CustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            var headers = new Dictionary<string, StringValues>();
            var body = string.Empty;
            var builder = new LearningProgressionBuilder("AccountEndpoint=https://dss-at-shared-cdb.documents.azure.com:443/;AccountKey=gN8fmksIp9YAz7a3GHARnMlr7EdA3MWg3vEVEHkIrWfqHhl9Mpxd651fwmgqfCEgmuFEaGDU3OJLNouL6zlbmQ==;");

            builder.WithHeader(headers)
            .WithBody(body)
            .WithNullLogger()
            .WithSubstituteValidator()
            .WithValidator()
            .WithCustomerId("1")
            .WithRequestHelper()
            .WithResponseMessageHelper()
            .WithJsonHelper()
            .WithCosmosDBClient()
            .WithResourceHelper()
            .WithLearningProgressionServiceWithCustomerExist();

            var httpPostFunction = new LearningProgressionPostTrigger(
                builder.LearnerProgressConfigurationSettings,
                builder.ResponseMessageHelper,
                builder.RequestHelper,
                builder.LearningProgressionServices,
                builder.JsonHelper,
                builder.ResourceHelper,
                builder.CosmosDBClient,
                builder.Validate
                );

            // Act
            var response = await httpPostFunction.Run(builder.Request, builder.Logger, builder.CustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            var headers = new Dictionary<string, StringValues>();
            var body = string.Empty;
            var builder = new LearningProgressionBuilder("AccountEndpoint=https://dss-at-shared-cdb.documents.azure.com:443/;AccountKey=gN8fmksIp9YAz7a3GHARnMlr7EdA3MWg3vEVEHkIrWfqHhl9Mpxd651fwmgqfCEgmuFEaGDU3OJLNouL6zlbmQ==;");

            builder.WithHeader(headers)
            .WithBody(body)
            .WithNullLogger()
            .WithSubstituteValidator()
            .WithValidator()
            .WithCustomerId("invalid guid")
            .WithRequestHelper()
            .WithResponseMessageHelper()            
            .WithJsonHelper()
            .WithCosmosDBClient()
            .WithResourceHelper()
            .WithLearningProgressionServiceWithCustomerNotExist();

            var httpPostFunction = new LearningProgressionPostTrigger(
                builder.LearnerProgressConfigurationSettings,
                builder.ResponseMessageHelper,
                builder.RequestHelper,
                builder.LearningProgressionServices,
                builder.JsonHelper,
                builder.ResourceHelper,
                builder.CosmosDBClient,
                builder.Validate
                );

            // Act
            var response = await httpPostFunction.Run(builder.Request, builder.Logger, builder.CustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        async Task Get_NoValidationErro_ReturnBadRequest()
        {
            // arrange
            var headers = new Dictionary<string, StringValues>();
            var body = string.Empty;
            var builder = new LearningProgressionBuilder("AccountEndpoint=https://dss-at-shared-cdb.documents.azure.com:443/;AccountKey=gN8fmksIp9YAz7a3GHARnMlr7EdA3MWg3vEVEHkIrWfqHhl9Mpxd651fwmgqfCEgmuFEaGDU3OJLNouL6zlbmQ==;");

            builder.WithHeader(headers)
            .WithBody(body)
            .WithNullLogger()
            .WithSubstituteValidator()
            .WithValidator()
            .WithCustomerId("invalid guid")
            .WithRequestHelper()
            .WithResponseMessageHelper()
            .WithLearningProgressionServiceWithCustomerNotExist()
            .WithJsonHelper()
            .WithCosmosDBClient()
            .WithResourceHelper();

            var httpPostFunction = new LearningProgressionPostTrigger(
                builder.LearnerProgressConfigurationSettings,
                builder.ResponseMessageHelper,
                builder.RequestHelper,
                builder.LearningProgressionServices,
                builder.JsonHelper,
                builder.ResourceHelper,
                builder.CosmosDBClient,
                builder.Validate
                );

            // Act
            var response = await httpPostFunction.Run(builder.Request, builder.Logger, builder.CustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }
    }
}
