using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.RoomService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.RoomExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class GetRoomsTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IRoomRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<RoomService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly RoomService _sut;

    public GetRoomsTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _mockRepo = _fixture.Freeze<Mock<IRoomRepository>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<RoomService>>>();
        _mockCache = _fixture.Freeze<Mock<IMemoryCache>>();

        _sut = new RoomService(
            _mockRepo.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCache.Object
        );
    }

    [Fact]
    [Trait("UnitTests - Room", "GetRooms")]
    public async Task Should_ReturnedDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var roomType = RoomType.Luxury;
        var cachedRooms = new List<RoomReadDto>
        {
            _fixture.Build<RoomReadDto>().With(x => x.RoomType, roomType).Create(),
            _fixture.Create<RoomReadDto>(),
            _fixture.Create<RoomReadDto>()
        };

        object cachedObject = cachedRooms;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(true);

        // Act
        var sut = await _sut.GetRoomsAsync();

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(cachedRooms[0].RoomType, sut[0].RoomType);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out cachedObject), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Room", "GetRooms")]
    public async Task Should_FailedToFetchRoomsException_When_ThereIsNoRoomsListCommingFromDatabase()
    {
        // Arrange
        var roomType = RoomType.Luxury;
        var cachedRooms = new List<RoomReadDto>
        {
            _fixture.Build<RoomReadDto>().With(x => x.RoomType, roomType).Create(),
            _fixture.Create<RoomReadDto>(),
            _fixture.Create<RoomReadDto>()
        };

        object cachedObject = cachedRooms;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync((List<Room>)null!);

        // Act & Assert 
        var exception = await Assert.ThrowsAsync<FailedToFetchRoomsException>(() => _sut.GetRoomsAsync());
        Assert.Equal(RoomServiceCustomMessages.FailedFetchingRoomsFromRepository, exception.Message);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Room", "GetRooms")]
    public async Task Should_ReturnedDataFromDatabase_When_ThereIsNoCachedData()
    {
        // Arrange
        var roomType = RoomType.Luxury;
        var cachedRooms = new List<RoomReadDto>
        {
            _fixture.Build<RoomReadDto>().With(x => x.RoomType, roomType).Create(),
            _fixture.Create<RoomReadDto>(),
            _fixture.Create<RoomReadDto>()
        };

        var rooms = new List<Room>
        {
            _fixture.Build<Room>().With(x => x.RoomType, roomType).Create(),
            _fixture.Create<Room>(),
            _fixture.Create<Room>()
        };

        object cachedObject = cachedRooms;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(rooms);
        _mockMapper.Setup(x => x.Map<List<RoomReadDto>>(It.IsAny<List<Room>>())).Returns(cachedRooms);

        // Act
        var sut = await _sut.GetRoomsAsync();

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(rooms[0].RoomType, sut[0].RoomType);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
        _mockRepo.Verify(x => x.GetAllAsync(), Times.Once);
    }
}