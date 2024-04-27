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
    public class RefreshTokenTest
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
        public async Task SuccessRefreshTokenTest()
        {
            //Arrange
            string accessToken = Guid.NewGuid().ToString(); 
            string refreshToken = Guid.NewGuid().ToString();

            var expectedResult = new AuthenticationResponse()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };

            var tokenDTO = new RefreshTokenDTO()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            _mocktokenGenerator.Setup(tg => tg.RefreshAccessToken(accessToken, refreshToken)).ReturnsAsync(expectedResult);

            //Act
            var result = await _userController.RefreshToken(tokenDTO);

            //Assert
            _mocktokenGenerator.Verify(tg => tg.RefreshAccessToken(accessToken, refreshToken), Times.Once());
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.That((result as ObjectResult)!.Value, Is.EqualTo(expectedResult));
        }

        [Test]
        public async Task FailedRefreshTokenTest()
        {
            //Arrange
            string accessToken = Guid.NewGuid().ToString();
            string refreshToken = Guid.NewGuid().ToString();

            var expectedResult = new AuthenticationResponse()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };

            var tokenDTO = new RefreshTokenDTO()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            var expectedError = new CustomApiException()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
                Detail = "Error occured while server running"
            };

            _mocktokenGenerator.Setup(tg => tg.RefreshAccessToken(accessToken, refreshToken)).ThrowsAsync(expectedError);

            //Act
            Assert.ThrowsAsync<CustomApiException>(async () => await _userController.RefreshToken(tokenDTO));

            //Assert
            _mocktokenGenerator.Verify(tg => tg.RefreshAccessToken(accessToken, refreshToken), Times.Once());
        }
    }
}
