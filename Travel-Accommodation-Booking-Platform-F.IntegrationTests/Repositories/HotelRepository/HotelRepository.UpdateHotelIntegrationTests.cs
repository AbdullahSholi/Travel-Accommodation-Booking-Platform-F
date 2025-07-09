using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.HotelService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class UpdateHotelIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private ICityRepository _cityRepository;
    private IHotelRepository _hotelRepository;
    private IHotelService _hotelService;
    private IMemoryCache _memoryCache;

    public UpdateHotelIntegrationTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var scope = Factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        _cityRepository = provider.GetRequiredService<ICityRepository>();
        _hotelRepository = provider.GetRequiredService<IHotelRepository>();
        _hotelService = provider.GetRequiredService<IHotelService>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Hotel", "UpdateHotel")]
    public async Task Should_UpdateHotelSuccessfully_When_CorrectDataProvided()
    {
        // Arrange
        var hotelName = "Al-Basha";
        var hotelsCacheKey = "hotels-list";

        await ClearDatabaseAsync();

        var cityMock = _fixture.Build<City>()
            .Without(c => c.CityId)
            .Without(c => c.Hotels)
            .Create();

        await SeedCitiesAsync(cityMock);

        var city = (await _cityRepository.GetAllAsync()).First();
        Assert.NotNull(city);

        var cityId = city.CityId;

        var hotelMock = _fixture.Build<Hotel>()
            .Without(h => h.HotelId)
            .Without(h => h.Rooms)
            .Without(h => h.Reviews)
            .With(h => h.CityId, cityId)
            .Create();

        await SeedHotelsAsync(hotelMock);

        var hotel = (await _hotelRepository.GetAllAsync()).First();
        Assert.NotNull(hotel);

        var hotelId = hotel.HotelId;

        var hotelPatchDto = _fixture.Build<HotelPatchDto>()
            .With(x => x.HotelName, hotelName)
            .Create();

        // Act
        await _hotelService.UpdateHotelAsync(hotelId, hotelPatchDto);

        // Assert
        var updatedHotel = await _hotelRepository.GetByIdAsync(hotelId);

        Assert.NotNull(updatedHotel);
        Assert.Equal(hotelPatchDto.HotelName, updatedHotel.HotelName);

        var cacheHit1 = _memoryCache.TryGetValue(hotelsCacheKey, out List<HotelReadDto> cachedHotels);
        var cacheHit2 = _memoryCache.TryGetValue(GetHotelCacheKey(hotelId), out HotelReadDto cachedHotel);

        Assert.False(cacheHit1);
        Assert.False(cacheHit2);
    }

    private string GetHotelCacheKey(int hotelId)
    {
        return $"hotel_{hotelId}";
    }
}