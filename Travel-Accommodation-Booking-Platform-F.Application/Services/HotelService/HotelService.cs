using AutoMapper;
using Microsoft.Extensions.Logging;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.HotelExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.HotelService;

public class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<HotelService> _logger;

    public HotelService(IHotelRepository hotelRepository, IMapper mapper, ILogger<HotelService> logger)
    {
        _hotelRepository = hotelRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HotelReadDto?> CreateHotelAsync(HotelWriteDto dto)
    {
        _logger.LogInformation(HotelServiceLogMessages.CreateHotelRequestReceived);

        if (dto == null)
        {
            _logger.LogWarning(HotelServiceLogMessages.InvalidHotelDataReceived);
            throw new InvalidHotelDataReceivedException(HotelServiceCustomMessages.InvalidHotelDataReceived);
        }

        _logger.LogInformation(HotelServiceLogMessages.CorrectHotelInformationSent);

        var hotel = _mapper.Map<Hotel>(dto);

        await _hotelRepository.AddAsync(hotel);

        var hotelReadDto = _mapper.Map<HotelReadDto>(hotel);
        return hotelReadDto;
    }

    public async Task<List<HotelReadDto>?> GetHotelsAsync()
    {
        _logger.LogInformation(HotelServiceLogMessages.FetchingHotelsFromRepository);

        var hotels = await _hotelRepository.GetAllAsync();
        if (hotels == null)
        {
            _logger.LogWarning(HotelServiceCustomMessages.FailedFetchingHotelsFromRepository);
            throw new FailedToFetchHotelsException(HotelServiceCustomMessages.FailedFetchingHotelsFromRepository);
        }

        _logger.LogInformation(HotelServiceLogMessages.FetchedHotelsFromRepositorySuccessfully);

        var hotelsReadDto = _mapper.Map<List<HotelReadDto>>(hotels);
        return hotelsReadDto;
    }

    public async Task<HotelReadDto?> GetHotelAsync(int hotelId)
    {
        _logger.LogInformation(HotelServiceLogMessages.GetHotelRequestReceived, hotelId);

        var hotel = await _hotelRepository.GetByIdAsync(hotelId);
        if (hotel == null) return null;

        _logger.LogInformation(HotelServiceLogMessages.FetchedHotelFromRepositorySuccessfully, hotelId);

        var hotelReadDto = _mapper.Map<HotelReadDto>(hotel);
        return hotelReadDto;
    }

    public async Task<HotelReadDto?> UpdateHotelAsync(int hotelId, HotelPatchDto dto)
    {
        _logger.LogInformation(HotelServiceLogMessages.UpdateHotelRequestReceived, hotelId);

        var hotel = await _hotelRepository.GetByIdAsync(hotelId);
        if (hotel == null) return null;

        _logger.LogInformation(HotelServiceLogMessages.RetrieveHotelSuccessfullyFromRepository, hotelId);

        hotel.HotelName = dto.HotelName ?? hotel.HotelName;
        hotel.OwnerName = dto.OwnerName ?? hotel.OwnerName;
        hotel.Location = dto.Location ?? hotel.Location;
        hotel.CityId = dto.CityId ?? hotel.CityId;
        hotel.LastUpdated = DateTime.UtcNow;

        await _hotelRepository.UpdateAsync(hotel);

        var hotelReadDto = _mapper.Map<HotelReadDto>(hotel);
        return hotelReadDto;
    }

    public async Task DeleteHotelAsync(int hotelId)
    {
        _logger.LogInformation(HotelServiceLogMessages.DeleteHotelRequestReceived, hotelId);

        var hotel = await _hotelRepository.GetByIdAsync(hotelId);
        if (hotel == null) return;
        _logger.LogInformation(HotelServiceLogMessages.RetrieveHotelSuccessfullyFromRepository, hotelId);


        await _hotelRepository.DeleteAsync(hotel);
        _logger.LogInformation(HotelServiceLogMessages.HotelDeletedSuccessfully, hotelId);
    }
}