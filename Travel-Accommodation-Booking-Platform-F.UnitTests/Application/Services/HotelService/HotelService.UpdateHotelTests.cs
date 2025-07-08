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
using Travel_Accommodation_Booking_Platform_F.Application.Services.CityService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class UpdateCityTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICityRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CityService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly CityService _sut;

    public UpdateCityTests()
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
    [Trait("UnitTests - City", "UpdateCity")]
    public async Task Should_ReturnNull_When_WeRequestInvalidCity()
    {
        // Arrange
        var cityId = 1;
        var cityName = "Nablus";
        var cityPatchDto = _fixture.Build<CityPatchDto>().With(x => x.Name, cityName).Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((City)null!);

        // Act
        var sut = await _sut.UpdateCityAsync(cityId, cityPatchDto);

        // Assert 
        Assert.Null(sut);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - City", "UpdateCity")]
    public async Task Should_UpdateCitySuccessfully_When_WeEnterValidDetails()
    {
        // Arrange
        var cityName = "Nablus";
        var cityId = 1;
        var cityPatchDto = _fixture.Build<CityPatchDto>().With(x => x.Name, cityName).Create();
        var cityReadDto = _fixture.Build<CityReadDto>().With(x => x.Name, cityName).Create();
        var city = _fixture.Build<City>().With(x => x.Name, cityName).Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(city);
        _mockRepo.Setup(x => x.UpdateAsync(It.IsAny<City>())).Returns(Task.CompletedTask);
        _mockCache.Setup(x => x.Remove(It.IsAny<string>()));
        _mockMapper.Setup(x => x.Map<CityReadDto>(It.IsAny<City>())).Returns(cityReadDto);

        // Act
        var sut = await _sut.UpdateCityAsync(cityId, cityPatchDto);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(cityReadDto.Name, sut.Name);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<City>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
        _mockMapper.Verify(x => x.Map<CityReadDto>(It.IsAny<City>()), Times.Once);
    }
}