using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService(IConfiguration config, IUserRepository userRepo) : IAuthService
{
    private readonly IConfiguration _config = config;
    private readonly IUserRepository _userRepo = userRepo;

    public AuthResponse Login(string email)
    {
        var user = _userRepo.GetByEmail(email)
            ?? throw new ArgumentException("Invalid email");

        var accessToken = GenerateJwt(user.Email, user.Role);

        var refreshToken = Guid.NewGuid().ToString();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        _userRepo.Update(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public AuthResponse RefreshToken(string refreshToken)
    {
        var user = _userRepo.GetByRefreshToken(refreshToken)
            ?? throw new ArgumentException("Invalid refresh token");

        if (user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new ArgumentException("Refresh token expired");

        var newAccessToken = GenerateJwt(user.Email, user.Role);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = refreshToken
        };
    }

    public void Logout(string refreshToken)
    {
        var user = _userRepo.GetByRefreshToken(refreshToken);
        if (user == null) return;

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        _userRepo.Update(user);
    }

    private string GenerateJwt(string email, string role)
    {
        var jwt = _config.GetSection("Jwt");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!)
        );

        var claims = new[]
        {
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Role, role)
    };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}