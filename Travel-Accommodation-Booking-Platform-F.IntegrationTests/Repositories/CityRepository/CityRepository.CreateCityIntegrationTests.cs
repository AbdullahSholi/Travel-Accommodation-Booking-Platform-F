using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.CityService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class CreateCityIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private ICityRepository _cityRepository;
    private ICityService _cityService;
    private IMapper _mapper;
    private IMemoryCache _memoryCache;

    public CreateCityIntegrationTests()
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
        _mapper = provider.GetRequiredService<IMapper>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }


    [Fact]
    [Trait("IntegrationTests - Admin", "CreateCity")]
    public async Task Should_AddNewCityCorrectly_When_CorrectCredentialsAreProvided()
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

        var cityWriteDto = _mapper.Map<CityWriteDto>(cityMock);

        // Act
        await _cityService.CreateCityAsync(cityWriteDto);

        // Assert
        var city = (await _cityRepository.GetAllAsync()).First();
        var cityId = city.CityId;

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