using Domain.Entities;

namespace Infrastructure.Persistence;

public static class AppDbContextSeed
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any())
            return; 

        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Ranveer Singh",
                Email = "ranveer@test.com"
            },
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john.doe@test.com"
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();
    }
}
