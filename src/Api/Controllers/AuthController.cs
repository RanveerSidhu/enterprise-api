using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

   
    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var result = _authService.Login(request.Email);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public IActionResult Refresh(RefreshTokenRequest request)
    {
        var result = _authService.RefreshToken(request.RefreshToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    public IActionResult Logout(RefreshTokenRequest request)
    {
        _authService.Logout(request.RefreshToken);
        return NoContent();
    }

}
