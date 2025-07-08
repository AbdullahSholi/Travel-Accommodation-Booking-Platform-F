using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.Services.CityService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class DeleteCityTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICityRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CityService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly CityService _sut;

    public DeleteCityTests()
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
    [Trait("UnitTests - City", "DeleteCity")]
    public async Task Should_FailedToDeleteCity_When_WeTryDeleteInvalidCity()
    {
        // Arrange
        var cityId = 1;

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((City)null!);

        // Act
        await _sut.DeleteCityAsync(cityId);

        // Assert 
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.DeleteAsync(It.IsAny<City>()), Times.Never);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("UnitTests - City", "DeleteCity")]
    public async Task Should_DeleteCitySuccessfully_When_WeTryDeleteValidCity()
    {
        // Arrange
        var cityId = 1;
        var cityName = "Nablus";
        var city = _fixture.Build<City>().With(x => x.Name, cityName).Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(city);

        // Act
        await _sut.DeleteCityAsync(cityId);

        // Assert 
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.DeleteAsync(It.IsAny<City>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
    }
}