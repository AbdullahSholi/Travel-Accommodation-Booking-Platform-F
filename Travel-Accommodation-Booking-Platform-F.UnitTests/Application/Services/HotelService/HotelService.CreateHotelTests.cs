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
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.CityExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class CreateCityTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICityRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CityService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly CityService _sut;

    public CreateCityTests()
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
    [Trait("UnitTests - City", "CreateCity")]
    public async Task Should_ThrowInvalidCityDataReceivedException_When_NullDtoRecieved()
    {
        // Arrange
        CityWriteDto? dto = null;
        
        // Act & ِAssert
        var exception = await Assert.ThrowsAsync<InvalidCityDataReceivedException>(() => _sut.CreateCityAsync(dto));

        Assert.Equal(CityServiceCustomMessages.InvalidCityDataReceived, exception.Message);
    }

    [Fact]
    [Trait("UnitTests - City", "CreateCity")]
    public async Task Should_AddedCitySuccessfully_When_ValidDataProvided()
    {
        // Arrange
        var writeDto = _fixture.Build<CityWriteDto>()
            .With(x => x.Name, "Nablus")
            .Create();

        var city = _fixture.Build<City>()
            .With(x => x.Name, "Nablus")
            .Create();

        var readDto = _fixture.Build<CityReadDto>()
            .With(x => x.Name, "Nablus")
            .Create();

        _mockMapper.Setup(x => x.Map<City>(It.IsAny<CityWriteDto>())).Returns(city);
        _mockRepo.Setup(x => x.AddAsync(city)).Returns(Task.CompletedTask);
        _mockCache.Setup(x => x.Remove(It.IsAny<string>()));
        _mockMapper.Setup(x => x.Map<CityReadDto>(city)).Returns(readDto);

        // Act
        var sut = await _sut.CreateCityAsync(writeDto);

        // Assert
        Assert.NotNull(sut);
        Assert.Equal(writeDto.Name, sut.Name);

        _mockMapper.Verify(x => x.Map<City>(It.IsAny<CityWriteDto>()), Times.Once);
        _mockRepo.Verify(x => x.AddAsync(It.IsAny<City>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
        _mockMapper.Verify(x => x.Map<CityReadDto>(It.IsAny<City>()), Times.Once);
    }
}