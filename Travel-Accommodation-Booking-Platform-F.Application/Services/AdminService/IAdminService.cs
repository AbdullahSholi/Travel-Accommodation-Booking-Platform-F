using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Domain.QueryDTOs;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.AdminService;

public interface IAdminService
{
    public Task<UserReadDto?> CreateUserAsync(UserWriteDto dto);
    public Task<List<UserReadDto>?> GetUsersAsync();
    public Task<UserReadDto?> GetUserAsync(int userId);
    public Task<UserReadDto?> UpdateUserAsync(int id, UserPatchDto dto);
    public Task DeleteUserAsync(int userId);
    public Task<List<CityReadDto>?> GetTopVisitedCitiesAsync();
    public Task<List<RoomReadDto>?> SearchRoomAsync(RoomQueryDto dto);
}