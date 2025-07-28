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
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class GetReviewTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IReviewRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ReviewService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly ReviewService _sut;

    public GetReviewTests()
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
    [Trait("UnitTests - Review", "GetReview")]
    public async Task Should_ReturnedDataFromCache_When_ThereIsValidDataAtCache()
    {
        // Arrange
        var rating = 3;
        var cachedReview = _fixture.Build<ReviewReadDto>()
            .With(x => x.ReviewId, 1)
            .With(x => x.Rating, rating)
            .Create();

        object cachedObject = cachedReview;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(true);

        // Act
        var sut = await _sut.GetReviewAsync(cachedReview.ReviewId);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(rating, sut.Rating);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out cachedObject), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Review", "GetReview")]
    public async Task Should_ReturnNull_When_WeTryRetrieveInvalidReview()
    {
        // Arrange
        var reviewId = 1;
        var rating = 3;
        var cachedReview = _fixture.Build<ReviewReadDto>().With(x => x.Rating, rating).Create();

        object cachedObject = cachedReview;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Review)null!);

        // Act
        var sut = await _sut.GetReviewAsync(reviewId);

        // Assert
        Assert.Null(sut);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Review", "GetReview")]
    public async Task Should_ReturnedDataFromDatabase_When_ThereIsNoCachedData()
    {
        // Arrange
        var reviewId = 1;
        var rating = 3;
        var cachedReview = _fixture.Build<ReviewReadDto>()
            .With(x => x.Rating, rating)
            .Create();

        var review = _fixture.Build<Review>()
            .With(x => x.Rating, rating)
            .Create();

        object cachedObject = cachedReview;
        _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedObject)).Returns(false);
        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(review);
        _mockMapper.Setup(x => x.Map<ReviewReadDto>(It.IsAny<Review>())).Returns(cachedReview);

        // Act
        var sut = await _sut.GetReviewAsync(reviewId);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(review.Rating, sut.Rating);
        _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Once);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
    }
}