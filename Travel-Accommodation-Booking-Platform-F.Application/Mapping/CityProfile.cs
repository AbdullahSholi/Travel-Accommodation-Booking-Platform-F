using AutoMapper;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;

namespace Travel_Accommodation_Booking_Platform_F.Application.Mapping;

public class CityProfile : Profile
{
    public CityProfile()
    {
        CreateMap<CityWriteDto, City>();
        CreateMap<CityPatchDto, City>();
        CreateMap<City, CityReadDto>();
        CreateMap<City, CityWriteDto>();
    }
}