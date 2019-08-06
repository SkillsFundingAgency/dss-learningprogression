using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using NSubstitute;
using DFC.HTTP.Standard;
using NCS.DSS.LearningProgression.GetLearningProgression.Service;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using DFC.Common.Standard.Logging;
using NCS.DSS.LearningProgression.CosmosDocumentClient;
using Microsoft.AspNetCore.Http;
using System;

namespace NCS.DSS.LearningProgression.Tests.FunctionTests
{
    public class LearningProgressionGetTriggerTests
    {
        public LearningProgressionGetTriggerTests()
        {
        }

        [Fact]
        async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            var LearningProgressionsGetTriggerService = Substitute.For<ILearningProgressionsGetTriggerService>();
            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionsGetTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionsGetTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("",""), TestFactory.CreateLogger(), "");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
             var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");

            var LearningProgressionsGetTriggerService = Substitute.For<ILearningProgressionsGetTriggerService>();
            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionsGetTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionsGetTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
              var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionsGetTriggerService = Substitute.For<ILearningProgressionsGetTriggerService>();
            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionsGetTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionsGetTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "InvalidCustomerId");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionsGetTriggerService = Substitute.For<ILearningProgressionsGetTriggerService>();
            var JsonHelper = new JsonHelper();

            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(false);

            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionsGetTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionsGetTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        async Task Get_RequestContainsNoErrors_ReturnOk()
        {
            // arrange
            var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionsGetTriggerService = Substitute.For<ILearningProgressionsGetTriggerService>();
            LearningProgressionsGetTriggerService.GetLearningProgressionsForCustomerAsync(Arg.Any<Guid>()).Returns<List<LearningProgression.Models.LearningProgression>>(new List<LearningProgression.Models.LearningProgression>());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(true);

            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionsGetTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionsGetTriggerService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
