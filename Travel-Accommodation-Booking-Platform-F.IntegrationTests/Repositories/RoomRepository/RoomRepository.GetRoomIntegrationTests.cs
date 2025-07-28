using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.RoomService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class GetRoomIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IRoomRepository _roomRepository;
    private ICityRepository _cityRepository;
    private IHotelRepository _hotelRepository;
    private IRoomService _roomService;
    private IMemoryCache _memoryCache;

    public GetRoomIntegrationTests()
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

        _roomRepository = provider.GetRequiredService<IRoomRepository>();
        _cityRepository = provider.GetRequiredService<ICityRepository>();
        _hotelRepository = provider.GetRequiredService<IHotelRepository>();
        _roomService = provider.GetRequiredService<IRoomService>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Room", "GetRoom")]
    public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
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

        var existingRoom = (await _roomRepository.GetAllAsync()).FirstOrDefault(u => u.RoomType == roomMock.RoomType);
        Assert.NotNull(existingRoom);

        var roomId = existingRoom.RoomId;

        // Act
        var room = await _roomService.GetRoomAsync(roomId);

        // Assert
        Assert.NotNull(room);

        var roomCacheKey = GetRoomCacheKey(roomId);

        var cacheHit = _memoryCache.TryGetValue(roomCacheKey, out RoomReadDto cachedRoom);
        Assert.True(cacheHit);
        Assert.NotNull(cachedRoom);
        Assert.Equal(room.RoomId, cachedRoom.RoomId);
    }

    [Fact]
    [Trait("IntegrationTests - Room", "GetRoom")]
    public async Task Should_ReturnDataFromDatabase_When_ThereIsNoValidDataAtCache()
    {
        // Arrange
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

        var existingRoom = (await _roomRepository.GetAllAsync()).First();
        var roomId = existingRoom.RoomId;
        var roomCacheKey = GetRoomCacheKey(roomId);

        var cacheHit = _memoryCache.TryGetValue(roomCacheKey, out RoomReadDto cachedRoom);
        Assert.False(cacheHit);

        // Act
        var room = await _roomService.GetRoomAsync(roomId);

        // Assert
        Assert.NotNull(room);

        cacheHit = _memoryCache.TryGetValue(roomCacheKey, out RoomReadDto cachedRoom1);
        Assert.True(cacheHit);
        Assert.NotNull(cachedRoom1);
    }

    private string GetRoomCacheKey(int roomId)
    {
        return $"room_{roomId}";
    }
}