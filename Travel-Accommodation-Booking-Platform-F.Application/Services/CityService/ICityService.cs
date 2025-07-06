using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.CityService;

public interface ICityService
{
    public Task<CityReadDto?> CreateCityAsync(CityWriteDto dto);
    public Task<List<CityReadDto>?> GetCitiesAsync();
    public Task<CityReadDto?> GetCityAsync(int cityId);
    public Task<CityReadDto?> UpdateCityAsync(int id, CityPatchDto dto);
    public Task DeleteCityAsync(int cityId);
}