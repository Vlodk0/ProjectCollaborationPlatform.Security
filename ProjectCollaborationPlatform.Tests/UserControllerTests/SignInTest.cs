using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectCollaborationPlatforn.Security.Controllers;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Helpers.ErrorFilter;
using ProjectCollaborationPlatforn.Security.Interfaces;
using ProjectCollaborationPlatforn.Security.Services.Autentication;

namespace ProjectCollaborationPlatform.Tests.UserControllerTests
{
    public class SignInTest
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
        public async Task SuccessSignInTest()
        {
            //Arrange
            var userDTO = new SignInDTO()
            {
                Email = "testUser@gmail.com",
                Password = "Us3r!Pass",
            };

            var token = new AuthenticationResponse
            {
                AccessToken = "dB_8.C4f-3.1KwL",
            };

            _mockUserService.Setup(us => us.IsUserExists(userDTO.Email)).ReturnsAsync(true);
            _mockUserService.Setup(us => us.CheckUserPassword(userDTO.Email, userDTO.Password)).ReturnsAsync(true);
            _mockUserService.Setup(us => us.GenerateTokens(userDTO.Email)).ReturnsAsync(token);

            //Act
            var result = await _userController.SignIn(userDTO);

            //Assert
            _mockUserService.Verify(us => us.IsUserExists(userDTO.Email), Times.Once);
            _mockUserService.Verify(us => us.CheckUserPassword(userDTO.Email, userDTO.Password), Times.Once);
            _mockUserService.Verify(us => us.GenerateTokens(userDTO.Email), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.That((result as ObjectResult)!.Value, Is.EqualTo(token));
        }

        [Test]
        public async Task FailedSignInTest()
        {
            //Arrange
            var userDTO = new SignInDTO()
            {
                Email = "testUser@gmail.com",
                Password = "Us3r!Pass",
            };

            _mockUserService.Setup(us => us.IsUserExists(userDTO.Email)).ReturnsAsync(false);

            //Act 
            Assert.ThrowsAsync<CustomApiException>(async () => await _userController.SignIn(userDTO));

            //Assert
            _mockUserService.Verify(us => us.IsUserExists(userDTO.Email), Times.Once);
        }

        [Test]
        public async Task FailedCheckUserPasswordSignInTest()
        {
            //Arrange
            var userDTO = new SignInDTO()
            {
                Email = "testUser@gmail.com",
                Password = "Us3r!Pass",
            };

            _mockUserService.Setup(us => us.IsUserExists(userDTO.Email)).ReturnsAsync(true);
            _mockUserService.Setup(us => us.CheckUserPassword(userDTO.Email, userDTO.Password)).ReturnsAsync(false);

            //Act 
            Assert.ThrowsAsync<CustomApiException>(async () => await _userController.SignIn(userDTO));

            //Assert
            _mockUserService.Verify(us => us.IsUserExists(userDTO.Email), Times.Once);
            _mockUserService.Verify(us => us.CheckUserPassword(userDTO.Email, userDTO.Password), Times.Once);
        }

        [Test]
        public async Task FailedGenerateTokensSignInTest()
        {
            //Arrange
            var userDTO = new SignInDTO()
            {
                Email = "testUser@gmail.com",
                Password = "Us3r!Pass",
            };

            _mockUserService.Setup(us => us.IsUserExists(userDTO.Email)).ReturnsAsync(true);
            _mockUserService.Setup(us => us.CheckUserPassword(userDTO.Email, userDTO.Password)).ReturnsAsync(true);
            _mockUserService.Setup(us => us.GenerateTokens(userDTO.Email)).ThrowsAsync(new CustomApiException()
            {
                StatusCode = StatusCodes.Status404NotFound,
                Title = "Not found",
                Detail = "Error ocurred while finding user by email"
            });

            //Act 
            Assert.ThrowsAsync<CustomApiException>(async () => await _userController.SignIn(userDTO));

            //Assert
            _mockUserService.Verify(us => us.IsUserExists(userDTO.Email), Times.Once);
            _mockUserService.Verify(us => us.CheckUserPassword(userDTO.Email, userDTO.Password), Times.Once);
            _mockUserService.Verify(us => us.GenerateTokens(userDTO.Email), Times.Once);
        }
    }
}
