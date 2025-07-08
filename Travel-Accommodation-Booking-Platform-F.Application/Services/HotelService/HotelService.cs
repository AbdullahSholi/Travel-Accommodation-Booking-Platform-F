using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.HotelExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Observer;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Subject;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.HotelService;

public class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<HotelService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IHotelPublisherSubject _hotelPublisherSubject;

    private const string HotelsCacheKey = "hotels-list";

    public HotelService(IHotelRepository hotelRepository, IMapper mapper, ILogger<HotelService> logger,
        IMemoryCache memoryCache, IHotelPublisherSubject hotelPublisherSubject,
        INotifyUsersObserver notifyUsersObserver)
    {
        _hotelRepository = hotelRepository;
        _mapper = mapper;
        _logger = logger;
        _memoryCache = memoryCache;
        _hotelPublisherSubject = hotelPublisherSubject;

        _hotelPublisherSubject.AddObserver(notifyUsersObserver);
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

        await _hotelPublisherSubject.NotifyObserversAsync(hotel);

        _logger.LogInformation(HotelServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(HotelsCacheKey);
        _memoryCache.Remove(GetHotelCacheKey(hotel.HotelId));

        var hotelReadDto = _mapper.Map<HotelReadDto>(hotel);
        return hotelReadDto;
    }

    public async Task<List<HotelReadDto>?> GetHotelsAsync()
    {
        _logger.LogInformation(HotelServiceLogMessages.FetchingHotelsFromRepository);

        if (_memoryCache.TryGetValue(HotelsCacheKey, out List<HotelReadDto> cachedHotels))
        {
            _logger.LogInformation(HotelServiceLogMessages.ReturningHotelsFromCache);
            return cachedHotels;
        }

        var hotels = await _hotelRepository.GetAllAsync();
        if (hotels == null)
        {
            _logger.LogWarning(HotelServiceCustomMessages.FailedFetchingHotelsFromRepository);
            throw new FailedToFetchHotelsException(HotelServiceCustomMessages.FailedFetchingHotelsFromRepository);
        }

        _logger.LogInformation(HotelServiceLogMessages.FetchedHotelsFromRepositorySuccessfully);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(Constants.AbsoluteExpirationForRetrieveHotelsMinutes))
            .SetSlidingExpiration(TimeSpan.FromMinutes(Constants.SlidingExpirationMinutes))
            .SetSize(Constants.CachingUnitSize);

        var hotelsReadDto = _mapper.Map<List<HotelReadDto>>(hotels);

        _memoryCache.Set(HotelsCacheKey, hotelsReadDto, cacheEntryOptions);
        return hotelsReadDto;
    }

    public async Task<HotelReadDto?> GetHotelAsync(int hotelId)
    {
        _logger.LogInformation(HotelServiceLogMessages.GetHotelRequestReceived, hotelId);

        var hotelCacheKey = GetHotelCacheKey(hotelId);

        if (_memoryCache.TryGetValue(hotelCacheKey, out HotelReadDto cachedHotel))
        {
            _logger.LogInformation(HotelServiceLogMessages.ReturningHotelFromCache);
            return cachedHotel;
        }

        var hotel = await _hotelRepository.GetByIdAsync(hotelId);
        if (hotel == null) return null;

        _logger.LogInformation(HotelServiceLogMessages.FetchedHotelFromRepositorySuccessfully, hotelId);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(Constants.AbsoluteExpirationForRetrieveHotelsMinutes))
            .SetSlidingExpiration(TimeSpan.FromMinutes(Constants.SlidingExpirationMinutes))
            .SetSize(Constants.CachingUnitSize);

        var hotelReadDto = _mapper.Map<HotelReadDto>(hotel);

        _memoryCache.Set(hotelCacheKey, hotelReadDto, cacheEntryOptions);
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
        hotel.Description = dto.Description ?? hotel.Description;
        hotel.LastUpdated = DateTime.UtcNow;

        await _hotelRepository.UpdateAsync(hotel);

        _logger.LogInformation(HotelServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(HotelsCacheKey);
        _memoryCache.Remove(GetHotelCacheKey(hotelId));

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

        _logger.LogInformation(HotelServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(HotelsCacheKey);
        _memoryCache.Remove(GetHotelCacheKey(hotelId));
    }

    private string GetHotelCacheKey(int hotelId)
    {
        return $"hotel_{hotelId}";
    }
}