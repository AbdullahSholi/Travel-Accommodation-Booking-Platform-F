using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.Services.ReviewService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class DeleteReviewTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IReviewRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ReviewService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly ReviewService _sut;

    public DeleteReviewTests()
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
    [Trait("UnitTests - Review", "DeleteReview")]
    public async Task Should_FailedToDeleteReview_When_WeTryDeleteInvalidReview()
    {
        // Arrange
        var reviewId = 1;

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Review)null!);

        // Act
        await _sut.DeleteReviewAsync(reviewId);

        // Assert 
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.DeleteAsync(It.IsAny<Review>()), Times.Never);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("UnitTests - Review", "DeleteReview")]
    public async Task Should_DeleteReviewSuccessfully_When_WeTryDeleteValidReview()
    {
        // Arrange
        var reviewId = 1;
        var rating = 3;
        var review = _fixture.Build<Review>().With(x => x.Rating, rating).Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(review);

        // Act
        await _sut.DeleteReviewAsync(reviewId);

        // Assert 
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.DeleteAsync(It.IsAny<Review>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
    }
}