using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.Services.RoomService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class DeleteRoomTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IRoomRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<RoomService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly RoomService _sut;

    public DeleteRoomTests()
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
    [Trait("UnitTests - Room", "DeleteRoom")]
    public async Task Should_FailedToDeleteRoom_When_WeTryDeleteInvalidRoom()
    {
        // Arrange
        var roomId = 1;

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Room)null!);

        // Act
        await _sut.DeleteRoomAsync(roomId);

        // Assert 
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.DeleteAsync(It.IsAny<Room>()), Times.Never);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("UnitTests - Room", "DeleteRoom")]
    public async Task Should_DeleteRoomSuccessfully_When_WeTryDeleteValidRoom()
    {
        // Arrange
        var roomId = 1;
        var roomType = RoomType.Luxury;
        var room = _fixture.Build<Room>().With(x => x.RoomType, roomType).Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(room);

        // Act
        await _sut.DeleteRoomAsync(roomId);

        // Assert 
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.DeleteAsync(It.IsAny<Room>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
    }
}