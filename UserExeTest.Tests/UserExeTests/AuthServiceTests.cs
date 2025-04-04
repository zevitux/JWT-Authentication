using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UserExe.Data;
using UserExe.Entities;
using UserExe.Models;
using UserExe.Services;

namespace UserExeTest.Tests.UserExeTests;

public class AuthServiceTests
{
    private readonly IAuthService _authService;
    private readonly User _existingUser;
    
    //Setting up a test environment
    public AuthServiceTests()
    {
        //Creates an InMemory database instance (does not persist between runs)
        var options = new DbContextOptionsBuilder<MyAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        var context = new MyAppDbContext(options);
        
        //False AppSettings for tests
        var inMemorySettings = new Dictionary<string, string>
        {
            { "AppSettings:Token", new string('x', 128) }, //Fake token
            { "AppSettings:Issuer", "TestIssuer" },
            { "AppSettings:Audience", "TestAudience" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
        
        //Create a mock ILogger<AuthService> so it doesn't depend on the real logger
        var loggerMock = new Mock<ILogger<AuthService>>();
        
        //Instantiate the real service with mocked/fake dependencies
        _authService = new AuthService(context, configuration, loggerMock.Object);
        
        //Create one test user to simulate real scenario in the database 
        _existingUser = new User
        {
            Name = "Test User",
            Email = "test@test.com",
            Role = "User",
            PasswordHash = new PasswordHasher<User>().HashPassword(null!, "123456")
        };
        
        context.Users.Add(_existingUser);
        context.SaveChanges(); // Save to InMemory DB
    }
    
    //Testing methods
    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
    {
        //Try to log in with email/password of user created on constructor
        var result = await _authService.LoginAsync(new LoginDto
        {
            Email = _existingUser.Email,
            Password = "123456"
        });
        
        //Validates if token was returned (result is not null)
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
    }

    [Fact]
    public async Task RegisterAsync_ReturnsNull_WhenEmailAlreadyExists()
    {
        //Tries to register a new user with the same email address as the existing one
        var result = await _authService.RegisterAsync(new RegisterDto
        {
            Name = "Another User",
            Email = _existingUser.Email,
            Password = "123456"
        });
        
        //Waits for AuthService to return null (registration not allowed)
        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshTokensAsync_ReturnsNull_WhenRefreshTokenIsInvalid()
    {
        //Passes an invalid refresh token (doesn't match the user's)
        var result = await _authService.RefreshTokensAsync(new RefreshTokenRequestDto
        {
            UserId = _existingUser.Id,
            RefreshToken = "invalid-token"
        });
        
        // Wait for AuthService to refuse and return null
        Assert.Null(result);
    }
}