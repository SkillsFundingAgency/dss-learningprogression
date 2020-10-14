using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.LearningProgression.PostLearningProgression.Service;
using NCS.DSS.LearningProgression.Validators;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.LearningProgression.Tests.FunctionTests
{
    [TestFixture]
    public class LearningProgressionPostTriggerTests
    {
        const string CustomerId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        const string InvalidCustomerId = "InvalidCustomerId";
        private Mock<ILogger> _log;
        private DefaultHttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Mock<ILearningProgressionPostTriggerService> _learningProgressionPatchTriggerService;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private LearningProgressionPostTrigger _function;
        private Mock<ILoggerHelper> _loggerHelper;
        private IValidate _validate;


        [SetUp]
        public void Setup()
        {
            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _loggerHelper = new Mock<ILoggerHelper>();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _learningProgressionPatchTriggerService = new Mock<ILearningProgressionPostTriggerService>();
            _jsonHelper = new JsonHelper();
            _validate = new Validate();
            _function = new LearningProgressionPostTrigger(
                _httpResponseMessageHelper,
                _httpRequestMessageHelper.Object,
                _learningProgressionPatchTriggerService.Object,
                _jsonHelper,
                _resourceHelper.Object,
                _validate,
                _loggerHelper.Object);
        }

        [Test]
        public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");

            // Act
            var response = await RunFunction(InvalidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_RequestContainsNoErrors_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _learningProgressionPatchTriggerService.Setup(x => x.CreateLearningProgressionAsync(new Models.LearningProgression())).Returns(Task.FromResult(new Models.LearningProgression()));

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_LearningProgressionAlreadyExist_ReturnConflict()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _learningProgressionPatchTriggerService.Setup(x => x.CreateLearningProgressionAsync(new Models.LearningProgression())).Returns(Task.FromResult(new Models.LearningProgression()));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _learningProgressionPatchTriggerService.Setup(x => x.DoesLearningProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Test]
        public async Task Get_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            var learningProgression = new Models.LearningProgression();
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.LearningProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(learningProgression));
            _learningProgressionPatchTriggerService.Setup(x => x.CreateLearningProgressionAsync(It.IsAny<Models.LearningProgression>())).Returns(Task.FromResult(learningProgression)); ;
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _learningProgressionPatchTriggerService.Setup(x => x.DoesLearningProgressionExistForCustomer(It.IsAny<Guid>())).Returns(false);

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Test]
        public async Task Get_UnableToCreateLearningProgression_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            var learningProgression = new Models.LearningProgression() { CustomerId = new Guid(CustomerId), DateProgressionRecorded = DateTime.Now.AddMonths(-1), CurrentQualificationLevel = ReferenceData.QualificationLevel.Level3, DateQualificationLevelAchieved = DateTime.Now.AddMonths(-2), CurrentLearningStatus = ReferenceData.CurrentLearningStatus.NotKnown };
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.LearningProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(learningProgression));
            _learningProgressionPatchTriggerService.Setup(x => x.CreateLearningProgressionAsync(It.IsAny<Models.LearningProgression>())).Returns(Task.FromResult<Models.LearningProgression>(null)); ;
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _learningProgressionPatchTriggerService.Setup(x => x.DoesLearningProgressionExistForCustomer(It.IsAny<Guid>())).Returns(false);

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_SuccessRequest_ReturnOk()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            var learningProgression = new Models.LearningProgression() { CustomerId = new Guid(CustomerId), DateProgressionRecorded = DateTime.Now.AddMonths(-1), CurrentQualificationLevel = ReferenceData.QualificationLevel.Level3, DateQualificationLevelAchieved = DateTime.Now.AddMonths(-2), CurrentLearningStatus = ReferenceData.CurrentLearningStatus.NotKnown };
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.LearningProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(learningProgression));
            _learningProgressionPatchTriggerService.Setup(x => x.CreateLearningProgressionAsync(It.IsAny<Models.LearningProgression>())).Returns(Task.FromResult(learningProgression)); ;
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _learningProgressionPatchTriggerService.Setup(x => x.DoesLearningProgressionExistForCustomer(It.IsAny<Guid>())).Returns(false);

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await _function.Run(_request, _log.Object, customerId).ConfigureAwait(false);
        }
    }
}