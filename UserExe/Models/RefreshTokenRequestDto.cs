namespace UserExe.Models;

public class RefreshTokenRequestDto
{
    public int UserId { get; set; }
    public required string RefreshToken { get; set; }
}