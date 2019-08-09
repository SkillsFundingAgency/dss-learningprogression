using System.Threading.Tasks;
using Xunit;
using System.Net;
using NSubstitute;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using DFC.Common.Standard.Logging;
using NCS.DSS.LearningProgression.CosmosDocumentClient;
using Microsoft.AspNetCore.Http;
using NCS.DSS.LearningProgression.PostLearningProgression.Service;
using NCS.DSS.LearningProgression.Validators;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using NCS.DSS.LearningProgression.Models;

namespace NCS.DSS.LearningProgression.Tests.FunctionTests
{
    public class LearningProgressionPostTriggerTests
    {
        [Fact]
        public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();

            var LearningProgressionPostTriggerService = Substitute.For<ILearningProgressionPostTriggerService>();
            LearningProgressionPostTriggerService.CreateLearningProgressionAsync(new Models.LearningProgression()).Returns(new Models.LearningProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPostTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");

            var LearningProgressionPostTriggerService = Substitute.For<ILearningProgressionPostTriggerService>();
            LearningProgressionPostTriggerService.CreateLearningProgressionAsync(new Models.LearningProgression()).Returns(new Models.LearningProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPostTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionPostTriggerService = Substitute.For<ILearningProgressionPostTriggerService>();
            LearningProgressionPostTriggerService.CreateLearningProgressionAsync(new Models.LearningProgression()).Returns(new Models.LearningProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPostTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "InvalidCustomerId");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionPostTriggerService = Substitute.For<ILearningProgressionPostTriggerService>();
            LearningProgressionPostTriggerService.CreateLearningProgressionAsync(new Models.LearningProgression()).Returns(new Models.LearningProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(false);

            var Valdiator = Substitute.For<IValidate>();
            
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPostTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_RequestContainsNoErrors_ReturnBadRequest()
        {
            // arrange
            
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionPostTriggerService = Substitute.For<ILearningProgressionPostTriggerService>();
            LearningProgressionPostTriggerService.CreateLearningProgressionAsync(new Models.LearningProgression()).Returns(new Models.LearningProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPostTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ReadOnlyCustomer_ReturnBadRequest()
        {
            // arrange
            
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionPostTriggerService = Substitute.For<ILearningProgressionPostTriggerService>();
            LearningProgressionPostTriggerService.CreateLearningProgressionAsync(new Models.LearningProgression()).Returns(new Models.LearningProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns<bool>(true);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(true);

            var Valdiator = Substitute.For<IValidate>();
            
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPostTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_LearningProgressionAlreadyExist_ReturnConflict()
        {
            // arrange
            
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionPostTriggerService = Substitute.For<ILearningProgressionPostTriggerService>();
            LearningProgressionPostTriggerService.CreateLearningProgressionAsync(new Models.LearningProgression()).Returns(new Models.LearningProgression());
            LearningProgressionPostTriggerService.DoesLearningProgressionExistForCustomer(Arg.Any<Guid>()).Returns<bool>(true);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns<bool>(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(true);

            var Valdiator = Substitute.For<IValidate>();
            
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPostTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Get_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange
            
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");

            var LearningProgressionPostTriggerService = Substitute.For<ILearningProgressionPostTriggerService>();
            LearningProgressionPostTriggerService.CreateLearningProgressionAsync(new Models.LearningProgression()).Returns(new Models.LearningProgression());
            LearningProgressionPostTriggerService.DoesLearningProgressionExistForCustomer(Arg.Any<Guid>()).Returns<bool>(false);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns<bool>(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(true);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();

            var validationResult = new ValidationResult("Error MEssage");
            ErrorResults.Add(validationResult);
            Valdiator.ValidateResource(Arg.Any<Models.LearningProgression>()).Returns(ErrorResults);

            
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPostTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Get_UnableToCreateLearningProgression_ReturnBadRequest()
        {
            // arrange
            
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");
            Models.LearningProgression learningProgression = new Models.LearningProgression();
            RequestHelper.GetResourceFromRequest<Models.LearningProgression>(Arg.Any<HttpRequest>()).Returns<Models.LearningProgression>(learningProgression);

            var LearningProgressionPostTriggerService = Substitute.For<ILearningProgressionPostTriggerService>();
            LearningProgressionPostTriggerService.CreateLearningProgressionAsync(new Models.LearningProgression()).Returns(new Models.LearningProgression());
            LearningProgressionPostTriggerService.DoesLearningProgressionExistForCustomer(Arg.Any<Guid>()).Returns<bool>(false);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns<bool>(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(true);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();
            Valdiator.ValidateResource(Arg.Any<Models.LearningProgression>()).Returns(ErrorResults);

            
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPostTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_SuccessRequest_ReturnOk()
        {
            // arrange
            
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns<string>("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns<string>("http://aurlvalue.com");
            var learningProgression = new Models.LearningProgression();
            RequestHelper.GetResourceFromRequest<Models.LearningProgression>(Arg.Any<HttpRequest>()).Returns<Models.LearningProgression>(learningProgression);

            var LearningProgressionPostTriggerService = Substitute.For<ILearningProgressionPostTriggerService>();
            LearningProgressionPostTriggerService.CreateLearningProgressionAsync(Arg.Any<Models.LearningProgression>()).Returns<Models.LearningProgression>(learningProgression);
            LearningProgressionPostTriggerService.DoesLearningProgressionExistForCustomer(Arg.Any<Guid>()).Returns<bool>(false);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns<bool>(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns<bool>(true);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();
            Valdiator.ValidateResource(Arg.Any<Models.LearningProgression>()).Returns(ErrorResults);

            
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new LearningProgressionPostTrigger(

                ResponseMessageHelper,
                RequestHelper,
                LearningProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
