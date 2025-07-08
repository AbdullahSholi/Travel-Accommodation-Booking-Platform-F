using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.RoomService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class UpdateRoomTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IRoomRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<RoomService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly RoomService _sut;

    public UpdateRoomTests()
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
    [Trait("UnitTests - Room", "UpdateRoom")]
    public async Task Should_ReturnNull_When_WeRequestInvalidRoom()
    {
        // Arrange
        var roomId = 1;
        var roomType = RoomType.Luxury;
        var roomPatchDto = _fixture.Build<RoomPatchDto>()
            .With(x => x.RoomType, roomType)
            .Without(x => x.PricePerNight)
            .Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Room)null!);

        // Act
        var sut = await _sut.UpdateRoomAsync(roomId, roomPatchDto);

        // Assert 
        Assert.Null(sut);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Room", "UpdateRoom")]
    public async Task Should_UpdateRoomSuccessfully_When_WeEnterValidDetails()
    {
        // Arrange
        var roomType = RoomType.Luxury;
        var roomId = 1;
        var roomPatchDto = _fixture.Build<RoomPatchDto>()
            .With(x => x.RoomType, roomType)
            .Without(x => x.PricePerNight)
            .Create();
        var roomReadDto = _fixture.Build<RoomReadDto>()
            .With(x => x.RoomType, roomType)
            .Without(x => x.PricePerNight)
            .Create();
        var room = _fixture.Build<Room>()
            .With(x => x.RoomType, roomType)
            .Without(x => x.PricePerNight)
            .Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(room);
        _mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Room>())).Returns(Task.CompletedTask);
        _mockCache.Setup(x => x.Remove(It.IsAny<string>()));
        _mockMapper.Setup(x => x.Map<RoomReadDto>(It.IsAny<Room>())).Returns(roomReadDto);

        // Act
        var sut = await _sut.UpdateRoomAsync(roomId, roomPatchDto);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(roomReadDto.RoomType, sut.RoomType);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Room>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
        _mockMapper.Verify(x => x.Map<RoomReadDto>(It.IsAny<Room>()), Times.Once);
    }
}