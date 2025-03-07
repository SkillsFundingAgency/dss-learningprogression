using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.LearningProgression.Cosmos.Helper;
using NCS.DSS.LearningProgression.PostLearningProgression.Function;
using NCS.DSS.LearningProgression.PostLearningProgression.Service;
using NCS.DSS.LearningProgression.Validators;
using NUnit.Framework;
using System.Net;

namespace NCS.DSS.LearningProgression.Tests.FunctionTests
{
    [TestFixture]
    public class LearningProgressionPostTriggerTests
    {
        private const string CustomerId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        private const string InvalidCustomerId = "InvalidCustomerId";

        private Mock<ILearningProgressionPostTriggerService> _learningProgressionPatchTriggerService = new();
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper = new();
        private Mock<IResourceHelper> _resourceHelper = new();
        private Mock<IDynamicHelper> _dynamicHelper = new();
        private Mock<ILogger<LearningProgressionPostTrigger>> _logger = new();

        private IValidate _validate = null!;
        private HttpRequest _request = null!;
        private LearningProgressionPostTrigger _function = null!;


        [SetUp]
        public void Setup()
        {
            _learningProgressionPatchTriggerService = new Mock<ILearningProgressionPostTriggerService>();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _resourceHelper = new Mock<IResourceHelper>();
            _validate = new Validate();
            _dynamicHelper = new Mock<IDynamicHelper>();
            _logger = new Mock<ILogger<LearningProgressionPostTrigger>>();

            _function = new LearningProgressionPostTrigger(
                _learningProgressionPatchTriggerService.Object,
                _httpRequestMessageHelper.Object,
                _resourceHelper.Object,
                _validate,
                _dynamicHelper.Object,
                _logger.Object);

            _request = new DefaultHttpContext().Request;
        }

        [Test]
        public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
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
            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnNotFound()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Get_RequestContainsNoErrors_ReturnNotFound()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _learningProgressionPatchTriggerService.Setup(x => x.CreateLearningProgressionAsync(new Models.LearningProgression())).Returns(Task.FromResult(new Models.LearningProgression())!);

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Get_LearningProgressionAlreadyExist_ReturnConflict()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _learningProgressionPatchTriggerService.Setup(x => x.CreateLearningProgressionAsync(new Models.LearningProgression())).Returns(Task.FromResult(new Models.LearningProgression())!);
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _learningProgressionPatchTriggerService.Setup(x => x.DoesLearningProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<ConflictObjectResult>());
        }

        [Test]
        public async Task Get_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            var learningProgression = new Models.LearningProgression();
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.LearningProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(learningProgression));
            _learningProgressionPatchTriggerService.Setup(x => x.CreateLearningProgressionAsync(It.IsAny<Models.LearningProgression>())).Returns(Task.FromResult(learningProgression)!);
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _learningProgressionPatchTriggerService.Setup(x => x.DoesLearningProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task Get_UnableToCreateLearningProgression_ReturnNotFound()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            var learningProgression = new Models.LearningProgression() { CustomerId = new Guid(CustomerId), DateProgressionRecorded = DateTime.Now.AddMonths(-1), CurrentQualificationLevel = ReferenceData.QualificationLevel.Level3, DateQualificationLevelAchieved = DateTime.Now.AddMonths(-2), CurrentLearningStatus = ReferenceData.CurrentLearningStatus.NotKnown };
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.LearningProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(learningProgression));
            _learningProgressionPatchTriggerService.Setup(x => x.CreateLearningProgressionAsync(It.IsAny<Models.LearningProgression>())).Returns(Task.FromResult<Models.LearningProgression>(null!)!);
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _learningProgressionPatchTriggerService.Setup(x => x.DoesLearningProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction(CustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Get_SuccessRequest_ReturnCreated()
        {
            // arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            var learningProgression = new Models.LearningProgression() { CustomerId = new Guid(CustomerId), DateProgressionRecorded = DateTime.Now.AddMonths(-1), CurrentQualificationLevel = ReferenceData.QualificationLevel.Level3, DateQualificationLevelAchieved = DateTime.Now.AddMonths(-2), CurrentLearningStatus = ReferenceData.CurrentLearningStatus.NotKnown };
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.LearningProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(learningProgression));
            _learningProgressionPatchTriggerService.Setup(x => x.CreateLearningProgressionAsync(It.IsAny<Models.LearningProgression>())).Returns(Task.FromResult(learningProgression)!);
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _learningProgressionPatchTriggerService.Setup(x => x.DoesLearningProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction(CustomerId);
            var responseResult = response as JsonResult;

            //Assert
            Assert.That(response, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult!.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        }

        private async Task<IActionResult> RunFunction(string customerId)
        {
            return await _function.Run(_request, customerId).ConfigureAwait(false);
        }
    }
}