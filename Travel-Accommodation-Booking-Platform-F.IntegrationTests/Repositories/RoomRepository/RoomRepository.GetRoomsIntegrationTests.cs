using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.RoomService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class GetRoomsIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private ICityRepository _cityRepository;
    private IHotelRepository _hotelRepository;
    private IRoomService _roomService;
    private IMemoryCache _memoryCache;

    public GetRoomsIntegrationTests()
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
        _hotelRepository = provider.GetService<IHotelRepository>();

        _roomService = provider.GetRequiredService<IRoomService>();
        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Room", "GetRooms")]
    public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var roomsCacheKey = "rooms-list";

        await ClearDatabaseAsync();

        var roomType = RoomType.Luxury;

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

        var roomMock = _fixture.Build<Room>()
            .Without(x => x.RoomId)
            .Without(x => x.Bookings)
            .With(x => x.HotelId, hotelId)
            .With(x => x.RoomType, roomType)
            .Create();

        await SeedRoomsAsync(roomMock);


        // Act
        var rooms = await _roomService.GetRoomsAsync();

        // Assert
        Assert.NotNull(rooms);
        Assert.Single(rooms);

        var cacheHit = _memoryCache.TryGetValue(roomsCacheKey, out List<RoomReadDto> cachedRooms);

        Assert.True(cacheHit);
        Assert.Equal(rooms.Count, cachedRooms.Count);
    }

    [Fact]
    [Trait("IntegrationTests - Room", "GetRooms")]
    public async Task Should_ReturnDataFromDatabase_When_ThereIsNoValidDataAtCache()
    {
        // Arrange
        var roomsCacheKey = "rooms-list";

        await ClearDatabaseAsync();

        var roomType = RoomType.Luxury;

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

        var roomMock = _fixture.Build<Room>()
            .Without(x => x.RoomId)
            .Without(x => x.Bookings)
            .With(x => x.HotelId, hotelId)
            .With(x => x.RoomType, roomType)
            .Create();

        await SeedRoomsAsync(roomMock);

        var cacheHit = _memoryCache.TryGetValue(roomsCacheKey, out List<RoomReadDto> cachedRooms);
        Assert.False(cacheHit);

        // Act
        var rooms = await _roomService.GetRoomsAsync();

        // Assert
        Assert.NotNull(rooms);

        cacheHit = _memoryCache.TryGetValue(roomsCacheKey, out List<RoomReadDto> cachedRooms1);
        Assert.True(cacheHit);
        Assert.True(cachedRooms1.Count == 1);
    }
}