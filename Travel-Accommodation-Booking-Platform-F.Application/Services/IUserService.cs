using Travel_Accommodation_Booking_Platform_F.Domain.Entities;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services;

public interface IUserService
{
    IEnumerable<User> GetUsers();
}