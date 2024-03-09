

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectCollaborationPlatforn.Security.Controllers;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Interfaces;
using ProjectCollaborationPlatforn.Security.Models;

namespace ProjectCollaborationPlatform.Tests.TokenGeneratorTests
{
    public class SignUpTest
    {
        private Mock<IUserService> _mockUserService;
        private Mock<ITokenGenerator> _mocktokenGenerator;

        [SetUp]
        public void SetUp()
        {
            _mockUserService = new Mock<IUserService>();
            _mocktokenGenerator = new Mock<ITokenGenerator>();
        }

        [TearDown]
        public void TearDown()
        {

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
            _mockUserService.Setup(us => us.AddUser(userDTO)).ReturnsAsync(expectedUser);
            var controller = new UserController(_mockUserService.Object, _mocktokenGenerator.Object);

            //Act
            var result = await controller.SignUp(userDTO);

            //Assert
            _mockUserService.Verify(um => um.IsUserExists(userDTO.Email), Times.Once);
            _mockUserService.Verify(um => um.AddUser(userDTO), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.That((result as ObjectResult)!.Value, Is.EqualTo(expectedUser));
        }
    }
}
