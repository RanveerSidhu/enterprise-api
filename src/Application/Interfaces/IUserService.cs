using Application.DTOs;

namespace Application.Interfaces;

public interface IUserService
{
    IEnumerable<UserDto> GetAllUsers();
    UserDto? GetUserById(Guid id);
    UserDto CreateUser(CreateUserRequest request);
    UserDto UpdateUser(Guid id, UpdateUserRequest request);
    void DeleteUser(Guid id);

}
