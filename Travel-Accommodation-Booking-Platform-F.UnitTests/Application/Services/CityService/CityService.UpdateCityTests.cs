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
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Observer;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Subject;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class UpdateHotelTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IHotelRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<HotelService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<IHotelPublisherSubject> _mockHotelPublisherSubject;
    private readonly Mock<INotifyUsersObserver> _mockNotifyUsersObserver;

    private readonly HotelService _sut;

    public UpdateHotelTests()
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
    [Trait("UnitTests - Hotel", "UpdateHotel")]
    public async Task Should_ReturnNull_When_WeRequestInvalidHotel()
    {
        // Arrange
        var hotelId = 1;
        var hotelName = "Al-Basha";
        var hotelPatchDto = _fixture.Build<HotelPatchDto>().With(x => x.HotelName, hotelName).Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Hotel)null!);

        // Act
        var sut = await _sut.UpdateHotelAsync(hotelId, hotelPatchDto);

        // Assert 
        Assert.Null(sut);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Hotel", "UpdateHotel")]
    public async Task Should_UpdateHotelSuccessfully_When_WeEnterValidDetails()
    {
        // Arrange
        var hotelName = "Al-Basha";
        var hotelId = 1;
        var hotelPatchDto = _fixture.Build<HotelPatchDto>().With(x => x.HotelName, hotelName).Create();
        var hotelReadDto = _fixture.Build<HotelReadDto>().With(x => x.HotelName, hotelName).Create();
        var hotel = _fixture.Build<Hotel>().With(x => x.HotelName, hotelName).Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(hotel);
        _mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Hotel>())).Returns(Task.CompletedTask);
        _mockCache.Setup(x => x.Remove(It.IsAny<string>()));
        _mockMapper.Setup(x => x.Map<HotelReadDto>(It.IsAny<Hotel>())).Returns(hotelReadDto);

        // Act
        var sut = await _sut.UpdateHotelAsync(hotelId, hotelPatchDto);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(hotelReadDto.HotelName, sut.HotelName);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Hotel>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
        _mockMapper.Verify(x => x.Map<HotelReadDto>(It.IsAny<Hotel>()), Times.Once);
    }
}