using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectCollaborationPlatforn.Security.Controllers;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Helpers.ErrorFilter;
using ProjectCollaborationPlatforn.Security.Interfaces;

namespace ProjectCollaborationPlatform.Tests.UserControllerTests
{
    public class VerifyingResetCodeTest
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
        public async Task SuccessVerifyingResetCodeTest()
        {
            //Arrange
            var resetCodeDTO = new ResetCodeDTO()
            {
                Email = "Test@gmail.com",
                NewPassword = "password",
                ResetCode = Guid.NewGuid().ToString(),
            };
            var userDTO = new UserDTO()
            {
                Id = Guid.NewGuid(),
                Email = "testUser@gmail.com",
                RoleName = "Test",
                UserName = "Test",
            };

            _mockUserService.Setup(us => us.IsUserExists(resetCodeDTO.Email)).ReturnsAsync(true);
            _mockUserService.Setup(us => us.GetUserByEmail(resetCodeDTO.Email)).ReturnsAsync(userDTO);
            _mockUserService.Setup(us => us.VerifyPasswordResetCode(userDTO, resetCodeDTO.ResetCode, 
                resetCodeDTO.NewPassword)).ReturnsAsync(true);

            //Act
            var result = await _userController.VerifyingResetCode(resetCodeDTO);

            //Assert
            _mockUserService.Verify(us => us.IsUserExists(resetCodeDTO.Email), Times.Once);
            _mockUserService.Verify(us => us.GetUserByEmail(resetCodeDTO.Email), Times.Once);
            _mockUserService.Verify(us => us.VerifyPasswordResetCode(userDTO, 
                resetCodeDTO.ResetCode, resetCodeDTO.NewPassword), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.That((result as OkObjectResult)!.Value, Is.EqualTo("Password reset successfully"));
        }

        [Test]
        public async Task FailedVerifyingResetCodeTest()
        {
            //Arrange
            var resetCodeDTO = new ResetCodeDTO()
            {
                Email = "Test@gmail.com",
                NewPassword = "password",
                ResetCode = Guid.NewGuid().ToString(),
            };
            var userDTO = new UserDTO()
            {
                Id = Guid.NewGuid(),
                Email = "testUser@gmail.com",
                RoleName = "Test",
                UserName = "Test",
            };


            _mockUserService.Setup(us => us.IsUserExists(resetCodeDTO.Email)).ReturnsAsync(false);

            //Act
            Assert.ThrowsAsync<CustomApiException>(async () => await _userController.VerifyingResetCode(resetCodeDTO));

            //Assert
            _mockUserService.Verify(us => us.IsUserExists(resetCodeDTO.Email), Times.Once);
        }

        [Test]
        public async Task FailedGetUserByEmailVerifyingResetCodeTest()
        {
            //Arrange
            var resetCodeDTO = new ResetCodeDTO()
            {
                Email = "Test@gmail.com",
                NewPassword = "password",
                ResetCode = Guid.NewGuid().ToString(),
            };
            var userDTO = new UserDTO()
            {
                Id = Guid.NewGuid(),
                Email = "testUser@gmail.com",
                RoleName = "Test",
                UserName = "Test",
            };


            _mockUserService.Setup(us => us.IsUserExists(resetCodeDTO.Email)).ReturnsAsync(true);
            _mockUserService.Setup(us => us.GetUserByEmail(resetCodeDTO.Email)).ReturnsAsync(new UserDTO());

            //Act
            Assert.ThrowsAsync<CustomApiException>(async () => await _userController.VerifyingResetCode(resetCodeDTO));

            //Assert
            _mockUserService.Verify(us => us.IsUserExists(resetCodeDTO.Email), Times.Once);
            _mockUserService.Verify(us => us.GetUserByEmail(resetCodeDTO.Email), Times.Once);
        }

        [Test]
        public async Task FailedVerifyPasswordResetCodeVerifyingResetCodeTest()
        {
            //Arrange
            var resetCodeDTO = new ResetCodeDTO()
            {
                Email = "Test@gmail.com",
                NewPassword = "password",
                ResetCode = Guid.NewGuid().ToString(),
            };
            var userDTO = new UserDTO()
            {
                Id = Guid.NewGuid(),
                Email = "testUser@gmail.com",
                RoleName = "Test",
                UserName = "Test",
            };


            _mockUserService.Setup(us => us.IsUserExists(resetCodeDTO.Email)).ReturnsAsync(true);
            _mockUserService.Setup(us => us.GetUserByEmail(resetCodeDTO.Email)).ReturnsAsync(userDTO);
            _mockUserService.Setup(us => us.VerifyPasswordResetCode(userDTO, 
                resetCodeDTO.ResetCode, resetCodeDTO.NewPassword)).ReturnsAsync(false);

            //Act
            Assert.ThrowsAsync<CustomApiException>(async () => await _userController.VerifyingResetCode(resetCodeDTO));

            //Assert
            _mockUserService.Verify(us => us.IsUserExists(resetCodeDTO.Email), Times.Once);
            _mockUserService.Verify(us => us.GetUserByEmail(resetCodeDTO.Email), Times.Once);
            _mockUserService.Verify(us => us.VerifyPasswordResetCode(userDTO, 
                resetCodeDTO.ResetCode, resetCodeDTO.NewPassword), Times.Once);
        }
    }
}
