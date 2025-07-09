using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.CityService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class GetCityIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private ICityRepository _cityRepository;
    private ICityService _cityService;
    private IMemoryCache _memoryCache;

    public GetCityIntegrationTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var scope = Factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        _cityRepository = provider.GetRequiredService<ICityRepository>();
        _cityService = provider.GetRequiredService<ICityService>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - City", "GetCity")]
    public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var cityName = "Nablus";
        var cityMock = _fixture.Build<City>()
            .Without(x => x.CityId)
            .Without(x => x.Hotels)
            .With(x => x.Name, cityName)
            .Create();

        await ClearDatabaseAsync();
        await SeedCitiesAsync(cityMock);

        var existingCity = (await _cityRepository.GetAllAsync()).FirstOrDefault(u => u.Name == cityMock.Name);
        Assert.NotNull(existingCity);

        var cityId = existingCity.CityId;

        // Act
        var city = await _cityService.GetCityAsync(cityId);

        // Assert
        Assert.NotNull(city);

        var cityCacheKey = GetCityCacheKey(cityId);

        var cacheHit = _memoryCache.TryGetValue(cityCacheKey, out CityReadDto cachedCity);
        Assert.True(cacheHit);
        Assert.NotNull(cachedCity);
        Assert.Equal(city.CityId, cachedCity.CityId);
    }

    [Fact]
    [Trait("IntegrationTests - City", "GetCity")]
    public async Task Should_ReturnDataFromDatabase_When_ThereIsNoValidDataAtCache()
    {
        // Arrange
        var cityMock = _fixture.Build<City>()
            .Without(x => x.CityId)
            .Without(x => x.Hotels)
            .Create();

        await ClearDatabaseAsync();
        await SeedCitiesAsync(cityMock);

        var existingCity = (await _cityRepository.GetAllAsync()).First();
        var cityId = existingCity.CityId;
        var cityCacheKey = GetCityCacheKey(cityId);

        var cacheHit = _memoryCache.TryGetValue(cityCacheKey, out CityReadDto cachedCity);
        Assert.False(cacheHit);

        // Act
        var city = await _cityService.GetCityAsync(cityId);

        // Assert
        Assert.NotNull(city);

        cacheHit = _memoryCache.TryGetValue(cityCacheKey, out CityReadDto cachedCity1);
        Assert.True(cacheHit);
        Assert.NotNull(cachedCity1);
    }

    private string GetCityCacheKey(int cityId)
    {
        return $"city_{cityId}";
    }
}