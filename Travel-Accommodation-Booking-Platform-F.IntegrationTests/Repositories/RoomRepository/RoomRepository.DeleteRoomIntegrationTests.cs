using System.Collections.Generic;
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

public class DeleteRoomIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IRoomRepository _adminRepository;
    private ICityRepository _cityRepository;
    private IHotelRepository _hotelRepository;
    private IRoomService _adminService;
    private IMemoryCache _memoryCache;

    public DeleteRoomIntegrationTests()
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

        _adminRepository = provider.GetRequiredService<IRoomRepository>();
        _cityRepository = provider.GetRequiredService<ICityRepository>();
        _hotelRepository = provider.GetRequiredService<IHotelRepository>();
        _adminService = provider.GetRequiredService<IRoomService>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Room", "DeleteRoom")]
    public async Task Should_DeleteRoomSuccessfully_When_CorrectDataProvided()
    {
        // Arrange
        await ClearDatabaseAsync();
        var roomType = RoomType.Luxury;
        var cacheKey = "rooms-list";

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

        var existingRoom = (await _adminRepository.GetAllAsync()).First();
        var roomId = existingRoom.RoomId;

        var room = await _adminRepository.GetByIdAsync(roomId);
        Assert.NotNull(room);

        // Act
        await _adminService.DeleteRoomAsync(roomId);

        // Assert
        var rooms = await _adminRepository.GetAllAsync();
        Assert.Empty(rooms);

        var cacheHit1 = _memoryCache.TryGetValue(cacheKey, out List<RoomReadDto> cachedRooms);
        var cacheHit2 = _memoryCache.TryGetValue(GetRoomCacheKey(roomId), out RoomReadDto cachedRoom);

        Assert.False(cacheHit1);
        Assert.False(cacheHit2);
    }

    private string GetRoomCacheKey(int roomId)
    {
        return $"room_{roomId}";
    }
}