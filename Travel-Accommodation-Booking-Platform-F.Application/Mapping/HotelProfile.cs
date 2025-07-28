using AutoMapper;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;

namespace Travel_Accommodation_Booking_Platform_F.Application.Mapping;

public class HotelProfile : Profile
{
    public HotelProfile()
    {
        CreateMap<HotelWriteDto, Hotel>();
        CreateMap<HotelPatchDto, Hotel>();
        CreateMap<Hotel, HotelReadDto>();
        CreateMap<Hotel, HotelWriteDto>();
    }
}