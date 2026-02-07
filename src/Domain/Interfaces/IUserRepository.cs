using Domain.Entities;

public interface IUserRepository
{
    IEnumerable<User> GetAll();
    User? GetById(Guid id);
    void Add(User user);

    void Update(User user);
    void Delete(User user);

    User? GetByEmail(string email);
    User? GetByRefreshToken(string refreshToken);
}
