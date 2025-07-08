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
using Travel_Accommodation_Booking_Platform_F.Application.Services.HotelService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.HotelExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Observer;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Subject;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class CreateHotelTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IHotelRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<HotelService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<IHotelPublisherSubject> _mockHotelPublisherSubject;
    private readonly Mock<INotifyUsersObserver> _mockNotifyUsersObserver;

    private readonly HotelService _sut;

    public CreateHotelTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _mockRepo = _fixture.Freeze<Mock<IHotelRepository>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<HotelService>>>();
        _mockCache = _fixture.Freeze<Mock<IMemoryCache>>();
        _mockHotelPublisherSubject = _fixture.Freeze<Mock<IHotelPublisherSubject>>();
        _mockNotifyUsersObserver = _fixture.Freeze<Mock<INotifyUsersObserver>>();

        _sut = new HotelService(
            _mockRepo.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCache.Object,
            _mockHotelPublisherSubject.Object,
            _mockNotifyUsersObserver.Object
        );
    }

    [Fact]
    [Trait("UnitTests - Hotel", "CreateHotel")]
    public async Task Should_ThrowInvalidHotelDataReceivedException_When_NullDtoRecieved()
    {
        // Arrange
        HotelWriteDto? dto = null;

        // Act & ِAssert
        var exception = await Assert.ThrowsAsync<InvalidHotelDataReceivedException>(() => _sut.CreateHotelAsync(dto));

        Assert.Equal(HotelServiceCustomMessages.InvalidHotelDataReceived, exception.Message);
    }

    [Fact]
    [Trait("UnitTests - Hotel", "CreateHotel")]
    public async Task Should_AddedHotelSuccessfully_When_ValidDataProvided()
    {
        // Arrange
        var writeDto = _fixture.Build<HotelWriteDto>()
            .With(x => x.HotelName, "Al-Basha")
            .Create();

        var hotel = _fixture.Build<Hotel>()
            .With(x => x.HotelName, "Al-Basha")
            .Create();

        var readDto = _fixture.Build<HotelReadDto>()
            .With(x => x.HotelName, "Al-Basha")
            .Create();

        _mockMapper.Setup(x => x.Map<Hotel>(It.IsAny<HotelWriteDto>())).Returns(hotel);
        _mockRepo.Setup(x => x.AddAsync(hotel)).Returns(Task.CompletedTask);
        _mockHotelPublisherSubject.Setup(x => x.NotifyObserversAsync(It.IsAny<Hotel>())).Returns(Task.CompletedTask);
        _mockCache.Setup(x => x.Remove(It.IsAny<string>()));
        _mockMapper.Setup(x => x.Map<HotelReadDto>(hotel)).Returns(readDto);

        // Act
        var sut = await _sut.CreateHotelAsync(writeDto);

        // Assert
        Assert.NotNull(sut);
        Assert.Equal(writeDto.HotelName, sut.HotelName);

        _mockMapper.Verify(x => x.Map<Hotel>(It.IsAny<HotelWriteDto>()), Times.Once);
        _mockHotelPublisherSubject.Verify(x => x.NotifyObserversAsync(It.IsAny<Hotel>()), Times.Once);
        _mockRepo.Verify(x => x.AddAsync(It.IsAny<Hotel>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
        _mockMapper.Verify(x => x.Map<HotelReadDto>(It.IsAny<Hotel>()), Times.Once);
    }
}