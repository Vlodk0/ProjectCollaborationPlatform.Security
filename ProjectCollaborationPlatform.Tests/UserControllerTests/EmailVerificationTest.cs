using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectCollaborationPlatforn.Security.Controllers;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Interfaces;
using ProjectCollaborationPlatforn.Security.Services.Autentication;

namespace ProjectCollaborationPlatform.Tests.UserControllerTests
{
    public class EmailVerificationTest
    {
        private Mock<IUserService> _mockUserService;
        private Mock<ITokenGenerator> _mocktokenGenerator;
        private UserController _userController;
        [SetUp]
        public void SetUp()
        {
            _mockUserService = new Mock<IUserService>();
            _mocktokenGenerator = new Mock<ITokenGenerator>();
            _userController = new UserController(_mockUserService.Object, _mocktokenGenerator.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _userController = null;
        }

        [Test]
        public async Task SuccessEmailVerificationTest()
        {
            //Arrange
            string id = Guid.NewGuid().ToString();
            string code = Guid.NewGuid().ToString();
            var userDTO = new UserDTO()
            {
                UserName = "User",
                Email = "testUser@gmail.com",
                RoleName = "Test",
            };

            var expectedResponse = "http://localhost:4200/email-success";


            _mockUserService.Setup(us => us.GetUserById(id)).ReturnsAsync(userDTO);
            _mockUserService.Setup(us => us.VerifyEmail(userDTO, code)).ReturnsAsync(true);

            //Act
            var result = await _userController.EmailVerification(id, code);

            //Assert 
            _mockUserService.Verify(us => us.GetUserById(id), Times.Once());
            _mockUserService.Verify(us => us.VerifyEmail(userDTO, code), Times.Once());
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<RedirectResult>(result);
            Assert.That(((RedirectResult)result).Url, Is.EqualTo(expectedResponse));

        }

        [Test]
        public async Task FailedVerificationTest()
        {
            string id = Guid.NewGuid().ToString();
            string code = Guid.NewGuid().ToString();
            var userDTO = new UserDTO()
            {
                UserName = "User",
                Email = "testUser@gmail.com",
                RoleName = "Test",
            };

            var expectedResponse = "http://localhost:4200/email-failed";


            _mockUserService.Setup(us => us.GetUserById(id)).ReturnsAsync(new UserDTO());

            //Act
            var result = await _userController.EmailVerification(id, code);

            //Assert
            _mockUserService.Verify(us => us.GetUserById(id), Times.Once());
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<RedirectResult>(result);
            Assert.That(((RedirectResult)result).Url, Is.EqualTo(expectedResponse));

        }

        [Test]
        public async Task FailedVerifyEmailEmailVerificationTest()
        {
            string id = Guid.NewGuid().ToString();
            string code = Guid.NewGuid().ToString();
            var userDTO = new UserDTO()
            {
                UserName = "User",
                Email = "testUser@gmail.com",
                RoleName = "Test",
            };

            var expectedResponse = "http://localhost:4200/email-failed";

            _mockUserService.Setup(us => us.GetUserById(id)).ReturnsAsync(userDTO);
            _mockUserService.Setup(us => us.VerifyEmail(userDTO, code)).ReturnsAsync(false);

            //Act
            var result = await _userController.EmailVerification(id, code);

            //Assert
            _mockUserService.Verify(us => us.GetUserById(id), Times.Once());
            _mockUserService.Verify(us => us.VerifyEmail(userDTO, code), Times.Once());
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<RedirectResult>(result);
            Assert.That(((RedirectResult)result).Url, Is.EqualTo(expectedResponse));
        }
    }
}
