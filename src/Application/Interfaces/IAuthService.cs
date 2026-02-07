using Application.DTOs;

namespace Application.Interfaces;

public interface IAuthService
{
    AuthResponse Login(string email);
    AuthResponse RefreshToken(string refreshToken);
    void Logout(string refreshToken);
}
