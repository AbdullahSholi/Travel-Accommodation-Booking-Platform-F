using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Repositories;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public IEnumerable<User> GetUsers()
    {
        return _userRepository.GetAllUsers();
    }
}