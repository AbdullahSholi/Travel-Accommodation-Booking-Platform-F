using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.ReviewService;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class UpdateReviewTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IReviewRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ReviewService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly ReviewService _sut;

    public UpdateReviewTests()
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
    [Trait("UnitTests - Review", "UpdateReview")]
    public async Task Should_ReturnNull_When_WeRequestInvalidReview()
    {
        // Arrange
        var reviewId = 1;
        var rating = 3;
        var reviewPatchDto = _fixture.Build<ReviewPatchDto>().With(x => x.Rating, rating).Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Review)null!);

        // Act
        var sut = await _sut.UpdateReviewAsync(reviewId, reviewPatchDto);

        // Assert 
        Assert.Null(sut);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("UnitTests - Review", "UpdateReview")]
    public async Task Should_UpdateReviewSuccessfully_When_WeEnterValidDetails()
    {
        // Arrange
        var rating = 3;
        var reviewId = 1;
        var reviewPatchDto = _fixture.Build<ReviewPatchDto>().With(x => x.Rating, rating).Create();
        var reviewReadDto = _fixture.Build<ReviewReadDto>().With(x => x.Rating, rating).Create();
        var review = _fixture.Build<Review>().With(x => x.Rating, rating).Create();

        _mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(review);
        _mockRepo.Setup(x => x.UpdateAsync(It.IsAny<Review>())).Returns(Task.CompletedTask);
        _mockCache.Setup(x => x.Remove(It.IsAny<string>()));
        _mockMapper.Setup(x => x.Map<ReviewReadDto>(It.IsAny<Review>())).Returns(reviewReadDto);

        // Act
        var sut = await _sut.UpdateReviewAsync(reviewId, reviewPatchDto);

        // Assert 
        Assert.NotNull(sut);
        Assert.Equal(reviewReadDto.Rating, sut.Rating);
        _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Review>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
        _mockMapper.Verify(x => x.Map<ReviewReadDto>(It.IsAny<Review>()), Times.Once);
    }
}