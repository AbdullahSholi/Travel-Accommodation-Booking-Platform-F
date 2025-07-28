using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.QueryDTOs;

namespace Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

public interface IAdminRepository : IRepository<User>
{
    public Task<bool> EmailExistsAsync(string email);
    public Task<List<City>> GetTopVisitedCitiesAsync();
    public Task<List<Room>> SearchRoomAsync(RoomQueryDto dto);
}