using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Repositories;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly List<User> _users = new()
    {
        new User { UserId = 1, Email = "abdullah.ghassan.sholi@gmail.com", Name = "Abdullah", Password = "123456" },
        new User { UserId = 2, Email = "abdullah.ghassan.sholi1@gmail.com", Name = "Abdullah", Password = "123456" },
        new User { UserId = 3, Email = "abdullah.ghassan.sholi2@gmail.com", Name = "Abdullah", Password = "123456" }
    };
    
    public IEnumerable<User> GetAllUsers()
    {
        return _users;
    }
}