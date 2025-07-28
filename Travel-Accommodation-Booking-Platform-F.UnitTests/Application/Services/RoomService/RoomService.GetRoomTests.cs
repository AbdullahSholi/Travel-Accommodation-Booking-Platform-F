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
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class GetRoomTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IRoomRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<RoomService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly RoomService _sut;

    public GetRoomTests()
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
    [Trait("UnitTests - Room", "GetRoom")]
    public async Task Should_ReturnedDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var roomType = RoomType.Luxury;
        var cachedRoom = _fixture.Build<RoomReadDto>()
            .With(x => x.RoomId, 1)
            .With(x => x.RoomType, roomType)
            .Create();

        object cachedObject = cachedRoom;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(true);

        // Act
        var sut = await _sut.GetRoomAsync(cachedRoom.RoomId);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(roomType, sut.RoomType);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out cachedObject), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Room", "GetRoom")]
    public async Task Should_ReturnNull_When_WeTryRetrieveInvalidRoom()
    {
        // Arrange
        var roomId = 1;
        var roomType = RoomType.Luxury;
        var cachedRoom = _fixture.Build<RoomReadDto>().With(x => x.RoomType, roomType).Create();

        object cachedObject = cachedRoom;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Room)null!);

        // Act
        var sut = await _sut.GetRoomAsync(roomId);

        // Assert
        Assert.Null(sut);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Room", "GetRoom")]
    public async Task Should_ReturnedDataFromDatabase_When_ThereIsNoCachedData()
    {
        // Arrange
        var roomId = 1;
        var roomType = RoomType.Luxury;
        var cachedRoom = _fixture.Build<RoomReadDto>()
            .With(x => x.RoomType, roomType)
            .Create();

        var room = _fixture.Build<Room>()
            .With(x => x.RoomType, roomType)
            .Create();

        object cachedObject = cachedRoom;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(room);
        _mockMapper.Setup(x => x.Map<RoomReadDto>(It.IsAny<Room>())).Returns(cachedRoom);

        // Act
        var sut = await _sut.GetRoomAsync(roomId);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(room.RoomType, sut.RoomType);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
    }
}