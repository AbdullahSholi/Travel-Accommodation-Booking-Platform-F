using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.RoomService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class CreateRoomIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IRoomRepository _roomRepository;
    private IHotelRepository _hotelRepository;
    private ICityRepository _cityRepository;
    private IRoomService _roomService;
    private IMapper _mapper;
    private IMemoryCache _memoryCache;

    public CreateRoomIntegrationTests()
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
        _hotelRepository = provider.GetRequiredService<IHotelRepository>();
        _cityRepository = provider.GetRequiredService<ICityRepository>();
        _roomService = provider.GetRequiredService<IRoomService>();
        _mapper = provider.GetRequiredService<IMapper>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }


    [Fact]
    [Trait("IntegrationTests - Room", "CreateRoom")]
    public async Task Should_AddNewRoomCorrectly_When_CorrectCredentialsAreProvided()
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

        var roomWriteDto = _mapper.Map<RoomWriteDto>(roomMock);

        // Act
        await _roomService.CreateRoomAsync(roomWriteDto);

        // Assert
        var room = (await _roomRepository.GetAllAsync()).First();
        var roomId = room.RoomId;

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