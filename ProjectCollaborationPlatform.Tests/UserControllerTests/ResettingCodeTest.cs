using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectCollaborationPlatforn.Security.Controllers;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Interfaces;

namespace ProjectCollaborationPlatform.Tests.UserControllerTests
{
    public class ResettingCodeTest
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
        public async Task SuccessResettingCodeTest()
        {
            //Arrange
            var emailDTO = new EmailDTO()
            {
                Subject = "Test",
                To = "Test",
            };

            var userDTO = new UserDTO()
            {
                UserName = "User",
                Email = "testUser@gmail.com",
                RoleName = "Test",
            };

            string expectedResult = "Reset code has sent!";

            _mockUserService.Setup(us => us.IsUserExists(emailDTO.To)).ReturnsAsync(true);
            _mockUserService.Setup(us => us.GetUserByEmail(emailDTO.To)).ReturnsAsync(userDTO);
            _mockUserService.Setup(us => us.SendPasswordResetCode(userDTO)).ReturnsAsync(true);

            //Act
            var result = await _userController.ResettingCode(emailDTO);

            //Assert
            _mockUserService.Verify(us => us.IsUserExists(emailDTO.To), Times.Once);
            _mockUserService.Verify(us => us.GetUserByEmail(emailDTO.To), Times.Once);
            _mockUserService.Verify(us => us.SendPasswordResetCode(userDTO), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.That((result as ObjectResult)!.Value, Is.EqualTo(expectedResult));
        }
    }
}
