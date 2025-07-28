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
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.RoomExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Enums;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class CreateRoomTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IRoomRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<RoomService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly RoomService _sut;

    public CreateRoomTests()
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
    [Trait("UnitTests - Room", "CreateRoom")]
    public async Task Should_ThrowInvalidRoomDataReceivedException_When_NullDtoRecieved()
    {
        // Arrange
        RoomWriteDto? dto = null;

        // Act & ِAssert
        var exception = await Assert.ThrowsAsync<InvalidRoomDataReceivedException>(() => _sut.CreateRoomAsync(dto));

        Assert.Equal(RoomServiceCustomMessages.InvalidRoomDataReceived, exception.Message);
    }

    [Fact]
    [Trait("UnitTests - Room", "CreateRoom")]
    public async Task Should_AddedRoomSuccessfully_When_ValidDataProvided()
    {
        // Arrange
        var writeDto = _fixture.Build<RoomWriteDto>()
            .With(x => x.RoomType, RoomType.Luxury)
            .Without(x => x.PricePerNight)
            .Create();

        var room = _fixture.Build<Room>()
            .With(x => x.RoomType, RoomType.Luxury)
            .Without(x => x.PricePerNight)
            .Create();

        var readDto = _fixture.Build<RoomReadDto>()
            .With(x => x.RoomType, RoomType.Luxury)
            .Without(x => x.PricePerNight)
            .Create();

        _mockMapper.Setup(x => x.Map<Room>(It.IsAny<RoomWriteDto>())).Returns(room);
        _mockRepo.Setup(x => x.AddAsync(room)).Returns(Task.CompletedTask);
        _mockCache.Setup(x => x.Remove(It.IsAny<string>()));
        _mockMapper.Setup(x => x.Map<RoomReadDto>(room)).Returns(readDto);

        // Act
        var sut = await _sut.CreateRoomAsync(writeDto);

        // Assert
        Assert.NotNull(sut);
        Assert.Equal(writeDto.RoomType, sut.RoomType);

        _mockMapper.Verify(x => x.Map<Room>(It.IsAny<RoomWriteDto>()), Times.Once);
        _mockRepo.Verify(x => x.AddAsync(It.IsAny<Room>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
        _mockMapper.Verify(x => x.Map<RoomReadDto>(It.IsAny<Room>()), Times.Once);
    }
}