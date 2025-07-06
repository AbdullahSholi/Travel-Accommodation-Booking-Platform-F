using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.HotelService;

public interface IHotelService
{
    public Task<HotelReadDto?> CreateHotelAsync(HotelWriteDto dto);
    public Task<List<HotelReadDto>?> GetHotelsAsync();
    public Task<HotelReadDto?> GetHotelAsync(int hotelId);
    public Task<HotelReadDto?> UpdateHotelAsync(int id, HotelPatchDto dto);
    public Task DeleteHotelAsync(int hotelId);
}