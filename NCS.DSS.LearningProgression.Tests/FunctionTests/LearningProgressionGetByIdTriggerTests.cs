using Microsoft.Extensions.Primitives;
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
using NCS.DSS.LearningProgression;
using NCS.DSS.LearningProgression.Tests;
using NCS.DSS.LearningProgression.GetLearningProgressionById.Service;

namespace NCS.DSS.LearningProgression.Tests.FunctionTests
{
    public class LearningProgressionGetByIdTriggerTests
    {
        public LearningProgressionGetByIdTriggerTests()
        {
        }

        [Fact]
        async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
             var LearnerProgressConfigurationSettings = new LearnerProgressConfigurationSettings();
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();

            var LearningProgressionGetByIdService = Substitute.For<ILearningProgressionGetByIdService>();
            LearningProgressionGetByIdService.GetLearningProgressionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(new LearningProgression.Models.LearningProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionGetByIdTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionGetByIdService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "","");

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

            var LearningProgressionGetByIdService = Substitute.For<ILearningProgressionGetByIdService>();
            LearningProgressionGetByIdService.GetLearningProgressionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(new LearningProgression.Models.LearningProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();


            var httpPostFunction = new LearningProgressionGetByIdTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionGetByIdService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "", "");

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

            var LearningProgressionGetByIdService = Substitute.For<ILearningProgressionGetByIdService>();
            LearningProgressionGetByIdService.GetLearningProgressionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(new LearningProgression.Models.LearningProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();


            var httpPostFunction = new LearningProgressionGetByIdTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionGetByIdService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "InvalidCustomerId", "");

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

            var LearningProgressionGetByIdService = Substitute.For<ILearningProgressionGetByIdService>();
            LearningProgressionGetByIdService.GetLearningProgressionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(new LearningProgression.Models.LearningProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(false);

            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionGetByIdTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionGetByIdService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33", "");

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

            var LearningProgressionGetByIdService = Substitute.For<ILearningProgressionGetByIdService>();
            LearningProgressionGetByIdService.GetLearningProgressionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(new LearningProgression.Models.LearningProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(true);

            var CosmosDBClient = Substitute.For<ICosmosDocumentClient>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionGetByIdTrigger(
                LearnerProgressConfigurationSettings,
                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionGetByIdService,
                JsonHelper,
                ResourceHelper,
                CosmosDBClient,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33", "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
