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
using Travel_Accommodation_Booking_Platform_F.Application.Services.CityService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.CityExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class GetCitiesTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICityRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CityService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly CityService _sut;

    public GetCitiesTests()
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
    [Trait("UnitTests - City", "GetCities")]
    public async Task Should_ReturnedDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var cityName = "Nablus";
        var cachedCities = new List<CityReadDto>
        {
            _fixture.Build<CityReadDto>().With(x => x.Name, cityName).Create(),
            _fixture.Create<CityReadDto>(),
            _fixture.Create<CityReadDto>()
        };

        object cachedObject = cachedCities;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(true);

        // Act
        var sut = await _sut.GetCitiesAsync();

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(cachedCities[0].Name, sut[0].Name);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out cachedObject), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - City", "GetCities")]
    public async Task Should_FailedToFetchCitiesException_When_ThereIsNoCitiesListCommingFromDatabase()
    {
        // Arrange
        var cityName = "Nablus";
        var cachedCities = new List<CityReadDto>
        {
            _fixture.Build<CityReadDto>().With(x => x.Name, cityName).Create(),
            _fixture.Create<CityReadDto>(),
            _fixture.Create<CityReadDto>()
        };

        object cachedObject = cachedCities;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync((List<City>)null!);

        // Act & Assert 
        var exception = await Assert.ThrowsAsync<FailedToFetchCitiesException>(() => _sut.GetCitiesAsync());
        Assert.Equal(CityServiceCustomMessages.FailedFetchingCitiesFromRepository, exception.Message);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - City", "GetCities")]
    public async Task Should_ReturnedDataFromDatabase_When_ThereIsNoCachedData()
    {
        // Arrange
        var cityName = "Nablus";
        var cachedCities = new List<CityReadDto>
        {
            _fixture.Build<CityReadDto>().With(x => x.Name, cityName).Create(),
            _fixture.Create<CityReadDto>(),
            _fixture.Create<CityReadDto>()
        };

        var cities = new List<City>
        {
            _fixture.Build<City>().With(x => x.Name, cityName).Create(),
            _fixture.Create<City>(),
            _fixture.Create<City>()
        };

        object cachedObject = cachedCities;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cities);
        _mockMapper.Setup(x => x.Map<List<CityReadDto>>(It.IsAny<List<City>>())).Returns(cachedCities);

        // Act
        var sut = await _sut.GetCitiesAsync();

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(cities[0].Name, sut[0].Name);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
        _mockRepo.Verify(x => x.GetAllAsync(), Times.Once);
    }
}