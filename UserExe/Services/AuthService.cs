using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserExe.Data;
using UserExe.Entities;
using UserExe.Models;

namespace UserExe.Services;

public class AuthService(MyAppDbContext context, IConfiguration configuration, ILogger<AuthService> logger) : IAuthService
{
    public async Task<TokenResponseDto?> LoginAsync(LoginDto request)
    {
        logger.LogInformation("Login login attempt for email: {Email}", request.Email);
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            logger.LogWarning("Login login attempt failed for email: {Email}", request.Email);   
            return null;
        }
        
        // Verify the provided password against the stored password hash
        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password!) ==
            PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Login login attempt failed for email: {Email}", request.Email);
            return null;
        }
        
        logger.LogInformation("User with e-mail: {Email} logged in successfully", request.Email);
        return await CreateTokenResponse(user); 
    }
    
    //Method to create token and refreshToken
    private async Task<TokenResponseDto?> CreateTokenResponse(User? user)
    {
        return new TokenResponseDto()
        {
            AccessToken = CreateToken(user),
            RefreshToken = (await GenerateAndSaveRefreshTokenAsync(user))!,
        };
    }
    
    //Method to handle user registration
    public async Task<User?> RegisterAsync(RegisterDto request)
    {
        logger.LogInformation("Register request attempt for email: {Email}", request.Email);
        //Check if email already exists
        if (await context.Users.AnyAsync(u => u.Email == request.Email))
        {
            logger.LogWarning("E-mail: {Email} already exists", request.Email);
            return null;
        }
        var adminEmail = configuration.GetSection("AppSettings:AdminEmails").Get<string[]>();
        bool isAdmin = adminEmail?.Contains(request.Email) ?? false;
        
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Role = isAdmin ? "Admin" : "User" // Admin only for specific emails
        };
        
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        //Generate and save the refresh token...
        await GenerateAndSaveRefreshTokenAsync(user);
        
        logger.LogInformation("User with e-mail: {Email} registered successfully", request.Email);
        return user;
    }

    public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
    {
        logger.LogInformation("Refresh token attempt for user with Id: {UserId}", request.UserId);
        
        var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
        if (user == null)
        {
            logger.LogWarning("Refresh token failed for user with Id: {UserId}", request.UserId);
            return null;
        }
        
        return await CreateTokenResponse(user);
    }

    private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
    {
        
        var user = await context.Users.FindAsync(userId);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            logger.LogWarning("Invalid refresh token for user with Id: {UserId}", userId);
            return null;
        }
        
        return user;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string?> GenerateAndSaveRefreshTokenAsync(User? user)
    {
        if(user == null)
            return null;
        
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        
        await context.SaveChangesAsync();
        
        logger.LogInformation("User with e-mail: {Email} generated successfully", user.Email);
        return refreshToken;
    }

    private string CreateToken(User? user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user!.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };
        
        
        var tokenKey = configuration.GetValue<string>("AppSettings:Token")
            ?? throw new Exception("Token key is missing in configuration");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
        
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("AppSettings:Issuer"),
            audience: configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddHours(10),
            signingCredentials: creds
        );
        
        logger.LogInformation("JWT Token created for user with e-mail: {Email}", user.Email);
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor); //Convert to string and return
    }
}
