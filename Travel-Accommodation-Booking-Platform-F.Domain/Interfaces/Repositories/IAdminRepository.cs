using Travel_Accommodation_Booking_Platform_F.Domain.Entities;

namespace Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

public interface IAdminRepository : IRepository<User>
{
    public Task<bool> EmailExistsAsync(string email);
}