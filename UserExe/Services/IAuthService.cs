using UserExe.Entities;
using UserExe.Models;

namespace UserExe.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(RegisterDto request);
    Task<TokenResponseDto?> LoginAsync(LoginDto request);
    Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
}