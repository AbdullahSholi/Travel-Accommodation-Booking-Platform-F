using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.RoomService;

public interface IRoomService
{
    public Task<RoomReadDto?> CreateRoomAsync(RoomWriteDto dto);
    public Task<List<RoomReadDto>?> GetRoomsAsync();
    public Task<RoomReadDto?> GetRoomAsync(int roomId);
    public Task<RoomReadDto?> UpdateRoomAsync(int id, RoomPatchDto dto);
    public Task DeleteRoomAsync(int roomId);
}