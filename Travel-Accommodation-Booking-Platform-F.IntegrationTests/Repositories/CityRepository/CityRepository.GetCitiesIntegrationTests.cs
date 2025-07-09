using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.CityService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

public class GetCitiesIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private ICityService _cityService;
    private IMemoryCache _memoryCache;

    public GetCitiesIntegrationTests()
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

        _cityService = provider.GetRequiredService<ICityService>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Admin", "GetCities")]
    public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var citiesCacheKey = "cities-list";

        await ClearDatabaseAsync();

        var city =
            _fixture.Build<City>()
                .Without(x => x.CityId)
                .Without(x => x.Hotels)
                .Create();
        await SeedCitiesAsync(city);

        // Act
        var cities = await _cityService.GetCitiesAsync();

        // Assert
        Assert.NotNull(cities);
        Assert.Single(cities);

        var cacheHit = _memoryCache.TryGetValue(citiesCacheKey, out List<CityReadDto> cachedCities);

        Assert.True(cacheHit);
        Assert.Equal(cities.Count, cachedCities.Count);
    }

    [Fact]
    [Trait("IntegrationTests - Admin", "GetCities")]
    public async Task Should_ReturnDataFromDatabase_When_ThereIsNoValidDataAtCache()
    {
        // Arrange
        var citiesCacheKey = "cities-list";
        var cityMock = _fixture.Build<City>()
            .Without(x => x.CityId)
            .Without(x => x.Hotels)
            .Create();

        _memoryCache.Remove(citiesCacheKey);
        await ClearDatabaseAsync();
        await SeedCitiesAsync(cityMock);


        var cacheHit = _memoryCache.TryGetValue(citiesCacheKey, out List<CityReadDto> cachedCities);
        Assert.False(cacheHit);

        // Act
        var cities = await _cityService.GetCitiesAsync();

        // Assert
        Assert.NotNull(cities);

        cacheHit = _memoryCache.TryGetValue(citiesCacheKey, out List<CityReadDto> cachedCities1);
        Assert.True(cacheHit);
        Assert.True(cachedCities1.Count == 1);
    }
}