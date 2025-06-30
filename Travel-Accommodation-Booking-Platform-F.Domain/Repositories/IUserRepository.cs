using Travel_Accommodation_Booking_Platform_F.Domain.Entities;

namespace Travel_Accommodation_Booking_Platform_F.Domain.Repositories;

public interface IUserRepository
{
    IEnumerable<User> GetAllUsers();
}