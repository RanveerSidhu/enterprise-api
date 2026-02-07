using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;

    public IEnumerable<UserDto> GetAllUsers()
    {
        return _userRepository.GetAll()
            .Select(user => new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            });
    }

    public UserDto? GetUserById(Guid id)
    {
        var user = _userRepository.GetById(id);

        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email
        };
    }

    public UserDto CreateUser(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Full name is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email
        };

        _userRepository.Add(user);

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email
        };
    }
    public UserDto UpdateUser(Guid id, UpdateUserRequest request)
    {
        var user = _userRepository.GetById(id)
            ?? throw new KeyNotFoundException("User not found");

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Full name is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");

        user.FullName = request.FullName;
        user.Email = request.Email;

        _userRepository.Update(user);

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email
        };
    }

    public void DeleteUser(Guid id)
    {
        var user = _userRepository.GetById(id)
            ?? throw new KeyNotFoundException("User not found");

        _userRepository.Delete(user);
    }
}
