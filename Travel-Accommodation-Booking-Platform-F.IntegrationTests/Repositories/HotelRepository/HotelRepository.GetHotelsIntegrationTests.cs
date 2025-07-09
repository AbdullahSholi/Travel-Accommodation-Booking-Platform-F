using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.HotelService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class GetHotelsIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private ICityRepository _cityRepository;
    private IHotelService _hotelService;
    private IMemoryCache _memoryCache;

    public GetHotelsIntegrationTests()
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

        _cityRepository = provider.GetService<ICityRepository>();

        _hotelService = provider.GetRequiredService<IHotelService>();
        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Hotel", "GetHotels")]
    public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
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

        // Act
        var hotels = await _hotelService.GetHotelsAsync();

        // Assert
        Assert.NotNull(hotels);
        Assert.Single(hotels);

        var cacheHit = _memoryCache.TryGetValue(hotelsCacheKey, out List<HotelReadDto> cachedHotels);

        Assert.True(cacheHit);
        Assert.Equal(hotels.Count, cachedHotels.Count);
    }

    [Fact]
    [Trait("IntegrationTests - Hotel", "GetHotels")]
    public async Task Should_ReturnDataFromDatabase_When_ThereIsNoValidDataAtCache()
    {
        // Arrange
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


        var cacheHit = _memoryCache.TryGetValue(hotelsCacheKey, out List<HotelReadDto> cachedHotels);
        Assert.False(cacheHit);

        // Act
        var hotels = await _hotelService.GetHotelsAsync();

        // Assert
        Assert.NotNull(hotels);

        cacheHit = _memoryCache.TryGetValue(hotelsCacheKey, out List<HotelReadDto> cachedHotels1);
        Assert.True(cacheHit);
        Assert.True(cachedHotels1.Count == 1);
    }
}