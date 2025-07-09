using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.ReviewService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class GetReviewsIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private ICityRepository _cityRepository;
    private IHotelRepository _hotelRepository;
    private IAdminRepository _adminRepository;
    private IReviewService _reviewService;
    private IMemoryCache _memoryCache;

    public GetReviewsIntegrationTests()
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

        _cityRepository = provider.GetService<ICityRepository>();
        _hotelRepository = provider.GetService<IHotelRepository>();
        _adminRepository = provider.GetService<IAdminRepository>();

        _reviewService = provider.GetRequiredService<IReviewService>();
        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Review", "GetReviews")]
    public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var reviewsCacheKey = "reviews-list";

        await ClearDatabaseAsync();

        var cityMock = _fixture.Build<City>()
            .Without(c => c.CityId)
            .Without(c => c.Hotels)
            .Create();

        await SeedCitiesAsync(cityMock);

        var city = (await _cityRepository.GetAllAsync()).First();
        Assert.NotNull(city);

        var cityId = city.CityId;

        var userMock = _fixture.Build<User>()
            .Without(u => u.UserId)
            .Without(u => u.OtpRecords)
            .Without(u => u.Bookings)
            .Without(u => u.Reviews)
            .Create();

        await SeedUsersAsync(userMock);

        var user = (await _adminRepository.GetAllAsync()).First();
        Assert.NotNull(user);

        var userId = user.UserId;

        var hotelMock = _fixture.Build<Hotel>()
            .Without(h => h.HotelId)
            .Without(h => h.Reviews)
            .Without(h => h.Reviews)
            .With(h => h.CityId, cityId)
            .Create();

        await SeedHotelsAsync(hotelMock);

        var hotel = (await _hotelRepository.GetAllAsync()).First();
        Assert.NotNull(hotel);

        var hotelId = hotel.HotelId;

        var reviewMock = _fixture.Build<Review>()
            .Without(x => x.ReviewId)
            .With(x => x.UserId, userId)
            .With(x => x.HotelId, hotelId)
            .Create();

        await SeedReviewsAsync(reviewMock);


        // Act
        var reviews = await _reviewService.GetReviewsAsync();

        // Assert
        Assert.NotNull(reviews);
        Assert.Single(reviews);

        var cacheHit = _memoryCache.TryGetValue(reviewsCacheKey, out List<ReviewReadDto> cachedReviews);

        Assert.True(cacheHit);
        Assert.Equal(reviews.Count, cachedReviews.Count);
    }

    [Fact]
    [Trait("IntegrationTests - Review", "GetReviews")]
    public async Task Should_ReturnDataFromDatabase_When_ThereIsNoValidDataAtCache()
    {
        // Arrange
        var reviewsCacheKey = "reviews-list";

        await ClearDatabaseAsync();

        var cityMock = _fixture.Build<City>()
            .Without(c => c.CityId)
            .Without(c => c.Hotels)
            .Create();

        await SeedCitiesAsync(cityMock);

        var city = (await _cityRepository.GetAllAsync()).First();
        Assert.NotNull(city);

        var cityId = city.CityId;

        var userMock = _fixture.Build<User>()
            .Without(u => u.UserId)
            .Without(u => u.OtpRecords)
            .Without(u => u.Bookings)
            .Without(u => u.Reviews)
            .Create();

        await SeedUsersAsync(userMock);

        var user = (await _adminRepository.GetAllAsync()).First();
        Assert.NotNull(user);

        var userId = user.UserId;

        var hotelMock = _fixture.Build<Hotel>()
            .Without(h => h.HotelId)
            .Without(h => h.Reviews)
            .Without(h => h.Reviews)
            .With(h => h.CityId, cityId)
            .Create();

        await SeedHotelsAsync(hotelMock);

        var hotel = (await _hotelRepository.GetAllAsync()).First();
        Assert.NotNull(hotel);

        var hotelId = hotel.HotelId;

        var reviewMock = _fixture.Build<Review>()
            .Without(x => x.ReviewId)
            .With(x => x.UserId, userId)
            .With(x => x.HotelId, hotelId)
            .Create();

        await SeedReviewsAsync(reviewMock);

        var cacheHit = _memoryCache.TryGetValue(reviewsCacheKey, out List<ReviewReadDto> cachedReviews);
        Assert.False(cacheHit);

        // Act
        var reviews = await _reviewService.GetReviewsAsync();

        // Assert
        Assert.NotNull(reviews);

        cacheHit = _memoryCache.TryGetValue(reviewsCacheKey, out List<ReviewReadDto> cachedReviews1);
        Assert.True(cacheHit);
        Assert.True(cachedReviews1.Count == 1);
    }
}