using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserExe.Controllers;
using UserExe.Entities;
using UserExe.Models;
using UserExe.Services;

namespace UserExeTest.Tests.UserExeTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            Mock<ILogger<AuthController>> loggerMock = new Mock<ILogger<AuthController>>();
            _authController = new AuthController(_authServiceMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenUserIsCreated()
        {
            //Arrange
            var request = new RegisterDto { Name = "Test User", Email = "test@example.com", Password = "password123" }; //Sample registration request
            var user = new User { Id = 1, Name = request.Name, Email = request.Email, Role = "User" }; //Expected user object after registration
            
            _authServiceMock.Setup(s => s.RegisterAsync(request)).ReturnsAsync(user);
            
            //Act
            var result = await _authController.Register(request); //Call register endpoint
            
            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(user, okResult.Value);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenEmailIsInUse()
        {
            //Arrange
            var request = new RegisterDto { Name = "Test User", Email = "test@example.com", Password = "password123" }; //Email is already taken
            _authServiceMock.Setup(s => s.RegisterAsync(request)).ReturnsAsync((User)null!);
            
            //Act
            var result = await _authController.Register(request); //Call register endpoint
            
            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            //Arrange
            var request = new LoginDto { Email = "test@example.com", Password = "password123" }; //Login request with valid credentials
            var tokenResponse = new TokenResponseDto { AccessToken = "valid-token", RefreshToken = "refresh-token" }; //Expected token response upon successful login
            
            _authServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync(tokenResponse); 
            
            //Act
            var result = await _authController.Login(request); //Call the login endpoint
            
            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(tokenResponse, okResult.Value);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenCredentialsAreInvalid()
        {
            //Arrange
            var request = new LoginDto { Email = "test@example.com", Password = "wrongpassword" }; //Login request with valid credentials
            _authServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync((TokenResponseDto)null!);
            
            //Act
            var result = await _authController.Login(request); //Call the login endpoint
            
            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task RefreshToken_ReturnsOk_WhenValid()
        {
            //Arrange
            var request = new RefreshTokenRequestDto { UserId = 1, RefreshToken = "valid-refresh-token" }; //Valid refresh token
            var tokenResponse = new TokenResponseDto { AccessToken = "new-access-token", RefreshToken = "new-refresh-token" }; //New token response
            
            _authServiceMock.Setup(s => s.RefreshTokensAsync(request)).ReturnsAsync(tokenResponse);
            
            //Act
            var result = await _authController.RefreshToken(request); //Call the RefreshToken endpoint
            
            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(tokenResponse, okResult.Value);
        }

        [Fact]
        public async Task RefreshToken_ReturnsUnauthorized_WhenInvalid()
        {
            //Arrange
            var request = new RefreshTokenRequestDto { UserId = 1, RefreshToken = "invalid-refresh-token" }; //Invalid refresh token
            _authServiceMock.Setup(s => s.RefreshTokensAsync(request)).ReturnsAsync((TokenResponseDto)null!);//Setup mock to return null indicating that the refresh token is invalid
            
            //Act
            var result = await _authController.RefreshToken(request); //Call the RefreshToken endpoint
            
            //Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }
    }
}