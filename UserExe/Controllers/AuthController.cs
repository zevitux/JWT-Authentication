using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserExe.Entities;
using UserExe.Models;
using UserExe.Services;

namespace UserExe.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(RegisterDto request)
    {
        var user = await authService.RegisterAsync(request);
        if(user == null)
            return BadRequest("Email already in use!");

        return Ok(user);
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login(LoginDto request)
    {
        var result = await authService.LoginAsync(request);
        if (result == null)
            return BadRequest("Email or password is incorrect!");
            
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
    {
        var result = await authService.RefreshTokensAsync(request);
        if(result == null || result.RefreshToken == null)
            return Unauthorized("Refresh tokens are invalid!");
        
        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public IActionResult AuthenticatedOnlyEndpoint()
    {
        return Ok("You are logged in!");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok("You are a admin!");
    }
}