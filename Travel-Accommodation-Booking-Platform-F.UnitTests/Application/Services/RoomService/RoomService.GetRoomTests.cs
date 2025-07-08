using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.CityService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class GetCityTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICityRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CityService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly CityService _sut;

    public GetCityTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _mockRepo = _fixture.Freeze<Mock<ICityRepository>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<CityService>>>();
        _mockCache = _fixture.Freeze<Mock<IMemoryCache>>();

        _sut = new CityService(
            _mockRepo.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCache.Object
        );
    }

    [Fact]
    [Trait("UnitTests - City", "GetCity")]
    public async Task Should_ReturnedDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var cityName = "Nablus";
        var cachedCity = _fixture.Build<CityReadDto>()
            .With(x => x.CityId, 1)
            .With(x => x.Name, cityName)
            .Create();

        object cachedObject = cachedCity;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(true);

        // Act
        var sut = await _sut.GetCityAsync(cachedCity.CityId);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(cityName, sut.Name);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out cachedObject), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - City", "GetCity")]
    public async Task Should_ReturnNull_When_WeTryRetrieveInvalidCity()
    {
        // Arrange
        var cityId = 1;
        var cityName = "Nablus";
        var cachedCity = _fixture.Build<CityReadDto>().With(x => x.Name, cityName).Create();

        object cachedObject = cachedCity;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((City)null!);

        // Act
        var sut = await _sut.GetCityAsync(cityId);

        // Assert
        Assert.Null(sut);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - City", "GetCity")]
    public async Task Should_ReturnedDataFromDatabase_When_ThereIsNoCachedData()
    {
        // Arrange
        var cityId = 1;
        var cityName = "Nablus";
        var cachedCity = _fixture.Build<CityReadDto>()
            .With(x => x.Name, cityName)
            .Create();

        var city = _fixture.Build<City>()
            .With(x => x.Name, cityName)
            .Create();

        object cachedObject = cachedCity;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(city);
        _mockMapper.Setup(x => x.Map<CityReadDto>(It.IsAny<City>())).Returns(cachedCity);

        // Act
        var sut = await _sut.GetCityAsync(cityId);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(city.Name, sut.Name);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
    }
}