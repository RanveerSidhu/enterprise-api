using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    public IEnumerable<User> GetAll()
    {
        return _context.Users
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .ToList();
    }

    public User? GetById(Guid id)
    {
        return _context.Users
            .AsNoTracking()
            .FirstOrDefault(u => u.Id == id);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }
    public void Update(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
        _context.SaveChanges();
    }
    public User? GetByEmail(string email)
    {
        return _context.Users.FirstOrDefault(u => u.Email == email);
    }

    public User? GetByRefreshToken(string refreshToken)
    {
        return _context.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);
    }

}
