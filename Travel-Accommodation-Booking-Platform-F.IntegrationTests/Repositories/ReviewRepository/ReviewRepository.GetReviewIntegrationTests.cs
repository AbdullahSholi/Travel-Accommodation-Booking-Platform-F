using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.ReviewService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class GetReviewIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IReviewRepository _reviewRepository;
    private ICityRepository _cityRepository;
    private IHotelRepository _hotelRepository;
    private IAdminRepository _adminRepository;
    private IReviewService _reviewService;
    private IMemoryCache _memoryCache;

    public GetReviewIntegrationTests()
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

        _reviewRepository = provider.GetRequiredService<IReviewRepository>();
        _cityRepository = provider.GetRequiredService<ICityRepository>();
        _hotelRepository = provider.GetRequiredService<IHotelRepository>();
        _adminRepository = provider.GetRequiredService<IAdminRepository>();
        _reviewService = provider.GetRequiredService<IReviewService>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }

    [Fact]
    [Trait("IntegrationTests - Review", "GetReview")]
    public async Task Should_ReturnDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
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

        var existingReview = (await _reviewRepository.GetAllAsync()).First();
        Assert.NotNull(existingReview);

        var reviewId = existingReview.ReviewId;

        // Act
        var review = await _reviewService.GetReviewAsync(reviewId);

        // Assert
        Assert.NotNull(review);

        var reviewCacheKey = GetReviewCacheKey(reviewId);

        var cacheHit = _memoryCache.TryGetValue(reviewCacheKey, out ReviewReadDto cachedReview);
        Assert.True(cacheHit);
        Assert.NotNull(cachedReview);
        Assert.Equal(review.ReviewId, cachedReview.ReviewId);
    }

    [Fact]
    [Trait("IntegrationTests - Review", "GetReview")]
    public async Task Should_ReturnDataFromDatabase_When_ThereIsNoValidDataAtCache()
    {
        // Arrange
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

        var existingReview = (await _reviewRepository.GetAllAsync()).First();
        Assert.NotNull(existingReview);

        var reviewId = existingReview.ReviewId;
        var reviewCacheKey = GetReviewCacheKey(reviewId);

        var cacheHit = _memoryCache.TryGetValue(reviewCacheKey, out ReviewReadDto cachedReview);
        Assert.False(cacheHit);

        // Act
        var review = await _reviewService.GetReviewAsync(reviewId);

        // Assert
        Assert.NotNull(review);

        cacheHit = _memoryCache.TryGetValue(reviewCacheKey, out ReviewReadDto cachedReview1);
        Assert.True(cacheHit);
        Assert.NotNull(cachedReview1);
    }

    private string GetReviewCacheKey(int reviewId)
    {
        return $"review_{reviewId}";
    }
}