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

public class DeleteHotelIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IHotelRepository _adminRepository;
    private ICityRepository _cityRepository;
    private IHotelService _adminService;
    private IMemoryCache _memoryCache;

    public DeleteHotelIntegrationTests()
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

        _adminRepository = provider.GetRequiredService<IHotelRepository>();
        _cityRepository = provider.GetRequiredService<ICityRepository>();
        _adminService = provider.GetRequiredService<IHotelService>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Hotel", "DeleteHotel")]
    public async Task Should_DeleteHotelSuccessfully_When_CorrectDataProvided()
    {
        // Arrange
        await ClearDatabaseAsync();
        var cacheKey = "hotels-list";

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

        var existingHotel = (await _adminRepository.GetAllAsync()).First();
        var hotelId = existingHotel.HotelId;

        var hotel = await _adminRepository.GetByIdAsync(hotelId);
        Assert.NotNull(hotel);

        // Act
        await _adminService.DeleteHotelAsync(hotelId);

        // Assert
        var hotels = await _adminRepository.GetAllAsync();
        Assert.Empty(hotels);

        var cacheHit1 = _memoryCache.TryGetValue(cacheKey, out List<HotelReadDto> cachedHotels);
        var cacheHit2 = _memoryCache.TryGetValue(GetHotelCacheKey(hotelId), out HotelReadDto cachedHotel);

        Assert.False(cacheHit1);
        Assert.False(cacheHit2);
    }

    private string GetHotelCacheKey(int hotelId)
    {
        return $"hotel_{hotelId}";
    }
}