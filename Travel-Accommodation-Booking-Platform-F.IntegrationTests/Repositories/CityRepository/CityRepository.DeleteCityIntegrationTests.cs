using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.CityService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class DeleteCityIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private ICityRepository _adminRepository;
    private ICityService _adminService;
    private IMemoryCache _memoryCache;

    public DeleteCityIntegrationTests()
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

        _adminRepository = provider.GetRequiredService<ICityRepository>();
        _adminService = provider.GetRequiredService<ICityService>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Admin", "DeleteCity")]
    public async Task Should_DeleteCitySuccessfully_When_CorrectDataProvided()
    {
        // Arrange
        await ClearDatabaseAsync();
        var cityName = "Nablus";
        var cacheKey = "cities-list";

        var cityMock = _fixture.Build<City>()
            .Without(x => x.CityId)
            .Without(x => x.Hotels)
            .With(x => x.Name, cityName)
            .Create();

        await SeedCitiesAsync(cityMock);

        var existingCity = (await _adminRepository.GetAllAsync()).First();
        var cityId = existingCity.CityId;

        var city = await _adminRepository.GetByIdAsync(cityId);
        Assert.NotNull(city);

        // Act
        await _adminService.DeleteCityAsync(cityId);

        // Assert
        var cities = await _adminRepository.GetAllAsync();
        Assert.Empty(cities);

        var cacheHit1 = _memoryCache.TryGetValue(cacheKey, out List<CityReadDto> cachedCities);
        var cacheHit2 = _memoryCache.TryGetValue(GetCityCacheKey(cityId), out CityReadDto cachedCity);

        Assert.False(cacheHit1);
        Assert.False(cacheHit2);
    }

    private string GetCityCacheKey(int cityId)
    {
        return $"city_{cityId}";
    }
}