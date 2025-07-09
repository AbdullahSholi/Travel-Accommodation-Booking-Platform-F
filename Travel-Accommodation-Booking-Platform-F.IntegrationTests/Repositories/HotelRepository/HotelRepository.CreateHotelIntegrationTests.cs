using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.HotelService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class CreateHotelIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IHotelRepository _hotelRepository;
    private ICityRepository _cityRepository;
    private IHotelService _hotelService;
    private IMapper _mapper;
    private IMemoryCache _memoryCache;

    public CreateHotelIntegrationTests()
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

        _hotelRepository = provider.GetRequiredService<IHotelRepository>();
        _cityRepository = provider.GetRequiredService<ICityRepository>();
        _hotelService = provider.GetRequiredService<IHotelService>();
        _mapper = provider.GetRequiredService<IMapper>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }


    [Fact]
    [Trait("IntegrationTests - Hotel", "CreateHotel")]
    public async Task Should_AddNewHotelCorrectly_When_CorrectCredentialsAreProvided()
    {
        // Arrange
        await ClearDatabaseAsync();
        var cacheKey = "hotels-list";

        var cityMock = _fixture.Build<City>()
            .Without(c => c.CityId)
            .Without(c => c.Hotels)
            .Create();

        await SeedCitiesAsync(cityMock);

        var city = (await _cityRepository.GetAllAsync()).First();
        Assert.NotNull(city);

        var cityId = city.CityId;

        var hotelMock = _fixture.Build<Hotel>()
            .Without(h => h.HotelId)
            .Without(h => h.Rooms)
            .Without(h => h.Reviews)
            .With(h => h.CityId, cityId)
            .Create();

        await SeedHotelsAsync(hotelMock);


        var hotelWriteDto = _mapper.Map<HotelWriteDto>(hotelMock);

        // Act
        await _hotelService.CreateHotelAsync(hotelWriteDto);

        // Assert
        var hotel = (await _hotelRepository.GetAllAsync()).First();
        var hotelId = hotel.HotelId;

        var cacheHit1 = _memoryCache.TryGetValue(cacheKey, out List<HotelReadDto> cachedHotels);
        var cacheHit2 = _memoryCache.TryGetValue(GetHotelCacheKey(hotelId), out HotelReadDto cachedHotel);
        Assert.False(cacheHit1);
        Assert.False(cacheHit2);
    }

    private string GetHotelCacheKey(int hotelId)
    {
        return $"hotel_{hotelId}";
    }
}