using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.CityExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.CityService;

public class CityService : ICityService
{
    private readonly ICityRepository _cityRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CityService> _logger;
    private readonly IMemoryCache _memoryCache;

    private const string CitiesCacheKey = "cities-list";
    private const string CityCacheKey = "city";

    public CityService(ICityRepository cityRepository, IMapper mapper, ILogger<CityService> logger,
        IMemoryCache memoryCache)
    {
        _cityRepository = cityRepository;
        _mapper = mapper;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<CityReadDto?> CreateCityAsync(CityWriteDto dto)
    {
        _logger.LogInformation(CityServiceLogMessages.CreateCityRequestReceived);

        if (dto == null)
        {
            _logger.LogWarning(CityServiceLogMessages.InvalidCityDataReceived);
            throw new InvalidCityDataReceivedException(CityServiceCustomMessages.InvalidCityDataReceived);
        }

        _logger.LogInformation(CityServiceLogMessages.CorrectCityInformationSent);

        var city = _mapper.Map<City>(dto);

        await _cityRepository.AddAsync(city);

        _logger.LogInformation(CityServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(CitiesCacheKey);
        _memoryCache.Remove(CityCacheKey);

        var cityReadDto = _mapper.Map<CityReadDto>(city);
        return cityReadDto;
    }

    public async Task<List<CityReadDto>?> GetCitiesAsync()
    {
        _logger.LogInformation(CityServiceLogMessages.FetchingCitiesFromRepository);

        if (_memoryCache.TryGetValue(CitiesCacheKey, out List<CityReadDto> cachedCities))
        {
            _logger.LogInformation(CityServiceLogMessages.ReturningCitiesFromCache);
            return cachedCities;
        }

        var cities = await _cityRepository.GetAllAsync();
        if (cities == null)
        {
            _logger.LogWarning(CityServiceCustomMessages.FailedFetchingCitiesFromRepository);
            throw new FailedToFetchCitiesException(CityServiceCustomMessages.FailedFetchingCitiesFromRepository);
        }

        _logger.LogInformation(CityServiceLogMessages.FetchedCitiesFromRepositorySuccessfully);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(Constants.AbsoluteExpirationForRetrieveCitiesMinutes))
            .SetSlidingExpiration(TimeSpan.FromMinutes(Constants.SlidingExpirationMinutes))
            .SetSize(Constants.CachingUnitSize);

        var citiesReadDto = _mapper.Map<List<CityReadDto>>(cities);

        _memoryCache.Set(CitiesCacheKey, citiesReadDto, cacheEntryOptions);
        return citiesReadDto;
    }

    public async Task<CityReadDto?> GetCityAsync(int cityId)
    {
        _logger.LogInformation(CityServiceLogMessages.GetCityRequestReceived, cityId);

        if (_memoryCache.TryGetValue(CityCacheKey, out CityReadDto cachedCity))
        {
            _logger.LogInformation(CityServiceLogMessages.ReturningCityFromCache);
            return cachedCity;
        }

        var city = await _cityRepository.GetByIdAsync(cityId);
        if (city == null) return null;

        _logger.LogInformation(CityServiceLogMessages.FetchedCityFromRepositorySuccessfully, cityId);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(Constants.AbsoluteExpirationForRetrieveCitiesMinutes))
            .SetSlidingExpiration(TimeSpan.FromMinutes(Constants.SlidingExpirationMinutes))
            .SetSize(Constants.CachingUnitSize);

        var cityReadDto = _mapper.Map<CityReadDto>(city);

        _memoryCache.Set(CityCacheKey, cityReadDto, cacheEntryOptions);
        return cityReadDto;
    }

    public async Task<CityReadDto?> UpdateCityAsync(int cityId, CityPatchDto dto)
    {
        _logger.LogInformation(CityServiceLogMessages.UpdateCityRequestReceived, cityId);

        var city = await _cityRepository.GetByIdAsync(cityId);
        if (city == null) return null;

        _logger.LogInformation(CityServiceLogMessages.RetrieveCitySuccessfullyFromRepository, cityId);

        city.Name = dto.Name ?? city.Name;
        city.Country = dto.Country ?? city.Country;
        city.PostOffice = dto.PostOffice ?? city.PostOffice;
        city.NumberOfHotels = dto.NumberOfHotels ?? city.NumberOfHotels;
        city.UpdatedAt = DateTime.UtcNow;
        city.LastUpdated = DateTime.UtcNow;

        await _cityRepository.UpdateAsync(city);

        _logger.LogInformation(CityServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(CitiesCacheKey);
        _memoryCache.Remove(CityCacheKey);

        var cityReadDto = _mapper.Map<CityReadDto>(city);
        return cityReadDto;
    }

    public async Task DeleteCityAsync(int cityId)
    {
        _logger.LogInformation(CityServiceLogMessages.DeleteCityRequestReceived, cityId);

        var city = await _cityRepository.GetByIdAsync(cityId);
        if (city == null) return;
        _logger.LogInformation(CityServiceLogMessages.RetrieveCitySuccessfullyFromRepository, cityId);


        await _cityRepository.DeleteAsync(city);
        _logger.LogInformation(CityServiceLogMessages.CityDeletedSuccessfully, cityId);

        _logger.LogInformation(CityServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(CitiesCacheKey);
        _memoryCache.Remove(CityCacheKey);
    }
}