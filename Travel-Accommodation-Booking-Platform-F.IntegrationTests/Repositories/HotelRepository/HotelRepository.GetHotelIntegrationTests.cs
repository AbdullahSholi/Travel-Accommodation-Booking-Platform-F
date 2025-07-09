using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.HotelService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class GetHotelIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private ICityRepository _cityRepository;
    private IHotelRepository _hotelRepository;
    private IHotelService _hotelService;
    private IMemoryCache _memoryCache;

    public GetHotelIntegrationTests()
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
    [Trait("IntegrationTests - Hotel", "GetHotel")]
    public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
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

        var existingHotel = (await _hotelRepository.GetAllAsync()).First();
        Assert.NotNull(existingHotel);

        var hotelId = existingHotel.HotelId;

        // Act
        var hotel = await _hotelService.GetHotelAsync(hotelId);

        // Assert
        Assert.NotNull(hotel);

        var hotelCacheKey = GetHotelCacheKey(hotelId);

        var cacheHit = _memoryCache.TryGetValue(hotelCacheKey, out HotelReadDto cachedHotel);
        Assert.True(cacheHit);
        Assert.NotNull(cachedHotel);
        Assert.Equal(hotel.HotelId, cachedHotel.HotelId);
    }

    [Fact]
    [Trait("IntegrationTests - Hotel", "GetHotel")]
    public async Task Should_ReturnDataFromDatabase_When_ThereIsNoValidDataAtCache()
    {
        // Arrange
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

        var existingHotel = (await _hotelRepository.GetAllAsync()).First();
        var hotelId = existingHotel.HotelId;
        var hotelCacheKey = GetHotelCacheKey(hotelId);

        var cacheHit = _memoryCache.TryGetValue(hotelCacheKey, out HotelReadDto cachedHotel);
        Assert.False(cacheHit);

        // Act
        var hotel = await _hotelService.GetHotelAsync(hotelId);

        // Assert
        Assert.NotNull(hotel);

        cacheHit = _memoryCache.TryGetValue(hotelCacheKey, out HotelReadDto cachedHotel1);
        Assert.True(cacheHit);
        Assert.NotNull(cachedHotel1);
    }

    private string GetHotelCacheKey(int hotelId)
    {
        return $"hotel_{hotelId}";
    }
}