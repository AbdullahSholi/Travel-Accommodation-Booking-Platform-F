using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.HotelService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Observer;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Subject;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class GetHotelTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IHotelRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<HotelService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<IHotelPublisherSubject> _mockHotelPublisherSubject;
    private readonly Mock<INotifyUsersObserver> _mockNotifyUsersObserver;

    private readonly HotelService _sut;

    public GetHotelTests()
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
    [Trait("UnitTests - Hotel", "GetHotel")]
    public async Task Should_ReturnedDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var hotelName = "Al-Basha";
        var cachedHotel = _fixture.Build<HotelReadDto>()
            .With(x => x.HotelId, 1)
            .With(x => x.HotelName, hotelName)
            .Create();

        object cachedObject = cachedHotel;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(true);

        // Act
        var sut = await _sut.GetHotelAsync(cachedHotel.HotelId);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(hotelName, sut.HotelName);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out cachedObject), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Hotel", "GetHotel")]
    public async Task Should_ReturnNull_When_WeTryRetrieveInvalidHotel()
    {
        // Arrange
        var hotelId = 1;
        var hotelName = "Al-Basha";
        var cachedHotel = _fixture.Build<HotelReadDto>().With(x => x.HotelName, hotelName).Create();

        object cachedObject = cachedHotel;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Hotel)null!);

        // Act
        var sut = await _sut.GetHotelAsync(hotelId);

        // Assert
        Assert.Null(sut);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Hotel", "GetHotel")]
    public async Task Should_ReturnedDataFromDatabase_When_ThereIsNoCachedData()
    {
        // Arrange
        var hotelId = 1;
        var hotelName = "Al-Basha";
        var cachedHotel = _fixture.Build<HotelReadDto>()
            .With(x => x.HotelName, hotelName)
            .Create();

        var hotel = _fixture.Build<Hotel>()
            .With(x => x.HotelName, hotelName)
            .Create();

        object cachedObject = cachedHotel;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(hotel);
        _mockMapper.Setup(x => x.Map<HotelReadDto>(It.IsAny<Hotel>())).Returns(cachedHotel);

        // Act
        var sut = await _sut.GetHotelAsync(hotelId);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(hotel.HotelName, sut.HotelName);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
    }
}