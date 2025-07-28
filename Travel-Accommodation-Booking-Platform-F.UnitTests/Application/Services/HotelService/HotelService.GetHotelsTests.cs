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
using Travel_Accommodation_Booking_Platform_F.Application.Services.HotelService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.HotelExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Observer;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.ObserverPattern.Subject;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class GetHotelsTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IHotelRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<HotelService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<IHotelPublisherSubject> _mockHotelPublisherSubject;
    private readonly Mock<INotifyUsersObserver> _mockNotifyUsersObserver;

    private readonly HotelService _sut;

    public GetHotelsTests()
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
    [Trait("UnitTests - Hotel", "GetHotels")]
    public async Task Should_ReturnedDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var hotelName = "Al-Basha";
        var cachedHotels = new List<HotelReadDto>
        {
            _fixture.Build<HotelReadDto>().With(x => x.HotelName, hotelName).Create(),
            _fixture.Create<HotelReadDto>(),
            _fixture.Create<HotelReadDto>()
        };

        object cachedObject = cachedHotels;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(true);

        // Act
        var sut = await _sut.GetHotelsAsync();

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(cachedHotels[0].HotelName, sut[0].HotelName);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out cachedObject), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Hotel", "GetHotels")]
    public async Task Should_FailedToFetchHotelsException_When_ThereIsNoHotelsListCommingFromDatabase()
    {
        // Arrange
        var hotelName = "Al-Basha";
        var cachedHotels = new List<HotelReadDto>
        {
            _fixture.Build<HotelReadDto>().With(x => x.HotelName, hotelName).Create(),
            _fixture.Create<HotelReadDto>(),
            _fixture.Create<HotelReadDto>()
        };

        object cachedObject = cachedHotels;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync((List<Hotel>)null!);

        // Act & Assert 
        var exception = await Assert.ThrowsAsync<FailedToFetchHotelsException>(() => _sut.GetHotelsAsync());
        Assert.Equal(HotelServiceCustomMessages.FailedFetchingHotelsFromRepository, exception.Message);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Hotel", "GetHotels")]
    public async Task Should_ReturnedDataFromDatabase_When_ThereIsNoCachedData()
    {
        // Arrange
        var hotelName = "Al-Basha";
        var cachedHotels = new List<HotelReadDto>
        {
            _fixture.Build<HotelReadDto>().With(x => x.HotelName, hotelName).Create(),
            _fixture.Create<HotelReadDto>(),
            _fixture.Create<HotelReadDto>()
        };

        var hotels = new List<Hotel>
        {
            _fixture.Build<Hotel>().With(x => x.HotelName, hotelName).Create(),
            _fixture.Create<Hotel>(),
            _fixture.Create<Hotel>()
        };

        object cachedObject = cachedHotels;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(hotels);
        _mockMapper.Setup(x => x.Map<List<HotelReadDto>>(It.IsAny<List<Hotel>>())).Returns(cachedHotels);

        // Act
        var sut = await _sut.GetHotelsAsync();

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(hotels[0].HotelName, sut[0].HotelName);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
        _mockRepo.Verify(x => x.GetAllAsync(), Times.Once);
    }
}