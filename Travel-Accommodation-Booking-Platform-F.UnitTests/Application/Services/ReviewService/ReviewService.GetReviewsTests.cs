using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.ReviewService;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.ReviewExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class GetReviewsTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IReviewRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ReviewService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly ReviewService _sut;

    public GetReviewsTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _mockRepo = _fixture.Freeze<Mock<IReviewRepository>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<ReviewService>>>();
        _mockCache = _fixture.Freeze<Mock<IMemoryCache>>();

        _sut = new ReviewService(
            _mockRepo.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCache.Object
        );
    }

    [Fact]
    [Trait("UnitTests - Review", "GetReviews")]
    public async Task Should_ReturnedDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var rating = 3;
        var cachedReviews = new List<ReviewReadDto>
        {
            _fixture.Build<ReviewReadDto>().With(x => x.Rating, rating).Create(),
            _fixture.Create<ReviewReadDto>(),
            _fixture.Create<ReviewReadDto>()
        };

        object cachedObject = cachedReviews;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(true);

        // Act
        var sut = await _sut.GetReviewsAsync();

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(cachedReviews[0].Rating, sut[0].Rating);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out cachedObject), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Review", "GetReviews")]
    public async Task Should_FailedToFetchReviewsException_When_ThereIsNoReviewsListCommingFromDatabase()
    {
        // Arrange
        var rating = 3;
        var cachedReviews = new List<ReviewReadDto>
        {
            _fixture.Build<ReviewReadDto>().With(x => x.Rating, rating).Create(),
            _fixture.Create<ReviewReadDto>(),
            _fixture.Create<ReviewReadDto>()
        };

        object cachedObject = cachedReviews;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync((List<Review>)null!);

        // Act & Assert 
        var exception = await Assert.ThrowsAsync<FailedToFetchReviewsException>(() => _sut.GetReviewsAsync());
        Assert.Equal(ReviewServiceCustomMessages.FailedFetchingReviewsFromRepository, exception.Message);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Review", "GetReviews")]
    public async Task Should_ReturnedDataFromDatabase_When_ThereIsNoCachedData()
    {
        // Arrange
        var rating = 3;
        var cachedReviews = new List<ReviewReadDto>
        {
            _fixture.Build<ReviewReadDto>().With(x => x.Rating, rating).Create(),
            _fixture.Create<ReviewReadDto>(),
            _fixture.Create<ReviewReadDto>()
        };

        var cities = new List<Review>
        {
            _fixture.Build<Review>().With(x => x.Rating, rating).Create(),
            _fixture.Create<Review>(),
            _fixture.Create<Review>()
        };

        object cachedObject = cachedReviews;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(cities);
        _mockMapper.Setup(x => x.Map<List<ReviewReadDto>>(It.IsAny<List<Review>>())).Returns(cachedReviews);

        // Act
        var sut = await _sut.GetReviewsAsync();

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(cities[0].Rating, sut[0].Rating);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
        _mockRepo.Verify(x => x.GetAllAsync(), Times.Once);
    }
}