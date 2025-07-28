using AutoMapper;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;

namespace Travel_Accommodation_Booking_Platform_F.Application.Mapping;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<ReviewWriteDto, Review>();
        CreateMap<ReviewPatchDto, Review>();
        CreateMap<Review, ReviewReadDto>();
        CreateMap<Review, ReviewWriteDto>();
    }
}