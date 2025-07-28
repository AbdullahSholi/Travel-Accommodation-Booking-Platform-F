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
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.ReviewExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;
using Xunit;

public class CreateReviewTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IReviewRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ReviewService>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockCache;

    private readonly ReviewService _sut;

    public CreateReviewTests()
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
    [Trait("UnitTests - Review", "CreateReview")]
    public async Task Should_ThrowInvalidReviewDataReceivedException_When_NullDtoRecieved()
    {
        // Arrange
        ReviewWriteDto? dto = null;

        // Act & ِAssert
        var exception = await Assert.ThrowsAsync<InvalidReviewDataReceivedException>(() => _sut.CreateReviewAsync(dto));

        Assert.Equal(ReviewServiceCustomMessages.InvalidReviewDataReceived, exception.Message);
    }

    [Fact]
    [Trait("UnitTests - Review", "CreateReview")]
    public async Task Should_AddedReviewSuccessfully_When_ValidDataProvided()
    {
        // Arrange
        var writeDto = _fixture.Build<ReviewWriteDto>()
            .With(x => x.Rating, 3)
            .Create();

        var review = _fixture.Build<Review>()
            .With(x => x.Rating, 3)
            .Create();

        var readDto = _fixture.Build<ReviewReadDto>()
            .With(x => x.Rating, 3)
            .Create();

        _mockMapper.Setup(x => x.Map<Review>(It.IsAny<ReviewWriteDto>())).Returns(review);
        _mockRepo.Setup(x => x.AddAsync(review)).Returns(Task.CompletedTask);
        _mockCache.Setup(x => x.Remove(It.IsAny<string>()));
        _mockMapper.Setup(x => x.Map<ReviewReadDto>(review)).Returns(readDto);

        // Act
        var sut = await _sut.CreateReviewAsync(writeDto);

        // Assert
        Assert.NotNull(sut);
        Assert.Equal(writeDto.Rating, sut.Rating);

        _mockMapper.Verify(x => x.Map<Review>(It.IsAny<ReviewWriteDto>()), Times.Once);
        _mockRepo.Verify(x => x.AddAsync(It.IsAny<Review>()), Times.Once);
        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(2));
        _mockMapper.Verify(x => x.Map<ReviewReadDto>(It.IsAny<Review>()), Times.Once);
    }
}