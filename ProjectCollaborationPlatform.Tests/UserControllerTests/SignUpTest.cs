using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectCollaborationPlatforn.Security.Controllers;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Helpers.ErrorFilter;
using ProjectCollaborationPlatforn.Security.Interfaces;

namespace ProjectCollaborationPlatform.Tests.UserControllerTests
{
    public class SignUpTest
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
        public async Task SuccessSignUpTestAsync()
        {
            //Arrange
            var userDTO = new SignUpDTO()
            {
                Name = "testUser",
                Email = "testUser@gmail.com",
                Password = "Us3r!Pass",
                RoleName = "Dev"
            };

            var expectedUser = new UserDTO
            {
                Email = "testUser@gmail.com"
            };

            _mockUserService.Setup(us => us.IsUserExists(userDTO.Email)).ReturnsAsync(false);
            _mockUserService.Setup(us => us.AddUser(userDTO)).ReturnsAsync(true);

            //Act
            var result = await _userController.SignUp(userDTO);

            //Assert
            _mockUserService.Verify(um => um.IsUserExists(userDTO.Email), Times.Once);
            _mockUserService.Verify(um => um.AddUser(userDTO), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.That((result as OkObjectResult)!.Value, Is.EqualTo("User created"));
        }

        [Test]
        public async Task FailedSignUpTestAsync()
        {
            //Arrange
            var userDTO = new SignUpDTO()
            {
                Name = "testUser",
                Email = "testUser@gmail.com",
                Password = "Us3r!Pass",
                RoleName = "Dev"
            };

            _mockUserService.Setup(us => us.IsUserExists(userDTO.Email)).ReturnsAsync(true);

            //Act
            Assert.ThrowsAsync<CustomApiException>(async () => await _userController.SignUp(userDTO));

            //Assert
            _mockUserService.Verify(um => um.IsUserExists(userDTO.Email), Times.Once);
        }

        [Test]
        public async Task FailedCreatingUserSignUpTestAsync()
        {
            //Arrange
            var userDTO = new SignUpDTO()
            {
                Name = "testUser",
                Email = "testUser@gmail.com",
                Password = "Us3r!Pass",
                RoleName = "Dev"
            };


            _mockUserService.Setup(us => us.IsUserExists(userDTO.Email)).ReturnsAsync(false);
            _mockUserService.Setup(us => us.AddUser(userDTO)).ThrowsAsync(new CustomApiException()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Title = "Error creating user",
                Detail = "User doesn't created"
            });

            //Act
            Assert.ThrowsAsync<CustomApiException>(async () => await _userController.SignUp(userDTO));

            //Assert
            _mockUserService.Verify(us => us.IsUserExists(userDTO.Email), Times.Once);
            _mockUserService.Verify(us => us.AddUser(userDTO), Times.Once);
        }
    }
}
