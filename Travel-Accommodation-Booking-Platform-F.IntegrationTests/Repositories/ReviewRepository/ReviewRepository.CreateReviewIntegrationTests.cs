using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.ReviewService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;
using Xunit.Abstractions;

public class CreateReviewIntegrationTests : IntegrationTestBase
{
    private readonly IFixture _fixture;
    private IReviewRepository _reviewRepository;
    private IHotelRepository _hotelRepository;
    private IAdminRepository _adminRepository;
    private ICityRepository _cityRepository;
    private IReviewService _reviewService;
    private IMapper _mapper;
    private IMemoryCache _memoryCache;

    public CreateReviewIntegrationTests()
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
        _hotelRepository = provider.GetRequiredService<IHotelRepository>();
        _adminRepository = provider.GetRequiredService<IAdminRepository>();
        _cityRepository = provider.GetRequiredService<ICityRepository>();
        _reviewService = provider.GetRequiredService<IReviewService>();
        _mapper = provider.GetRequiredService<IMapper>();

        _memoryCache = provider.GetRequiredService<IMemoryCache>();
    }


    [Fact]
    [Trait("IntegrationTests - Review", "CreateReview")]
    public async Task Should_AddNewReviewCorrectly_When_CorrectCredentialsAreProvided()
    {
        // Arrange
        await ClearDatabaseAsync();
        var cacheKey = "reviews-list";

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
            .Without(h => h.Rooms)
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

        var reviewWriteDto = _mapper.Map<ReviewWriteDto>(reviewMock);

        // Act
        await _reviewService.CreateReviewAsync(reviewWriteDto);

        // Assert
        var review = (await _reviewRepository.GetAllAsync()).First();
        var reviewId = review.ReviewId;

        var cacheHit1 = _memoryCache.TryGetValue(cacheKey, out List<ReviewReadDto> cachedReviews);
        var cacheHit2 = _memoryCache.TryGetValue(GetReviewCacheKey(reviewId), out ReviewReadDto cachedReview);
        Assert.False(cacheHit1);
        Assert.False(cacheHit2);
    }

    private string GetReviewCacheKey(int reviewId)
    {
        return $"review_{reviewId}";
    }
}