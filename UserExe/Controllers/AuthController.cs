using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserExe.Entities;
using UserExe.Models;
using UserExe.Services;

namespace UserExe.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(RegisterDto request)
    {
        var user = await _authService.RegisterAsync(request);
        if (user == null)
            return BadRequest("Email already in use!");
        
        return Ok(user);
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login(LoginDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
            return BadRequest("E-mail or password is incorrect!");
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokensAsync(request);
        if(result == null)
            return Unauthorized("Refresh tokens are invalid!");
        
        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public IActionResult AuthenticatedOnlyEndpoint()
    {
        _logger.LogInformation("Authenticated endpoint accessed by the user: {User}", User.Identity?.Name);
        return Ok("You are logged in!");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnlyEndpoint()
    {
        _logger.LogInformation("Admin endpoint accessed by the user: {User}", User.Identity?.Name);
        return Ok("You are a admin!");
    }
}