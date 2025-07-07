using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.ReadDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.CustomMessages;
using Travel_Accommodation_Booking_Platform_F.Application.Utils.LogMessages;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.ReviewExceptions;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Domain.Interfaces.Repositories;

namespace Travel_Accommodation_Booking_Platform_F.Application.Services.ReviewService;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewService> _logger;
    private readonly IMemoryCache _memoryCache;

    private const string ReviewsCacheKey = "reviews-list";
    private const string ReviewCacheKey = "review";

    public ReviewService(IReviewRepository reviewRepository, IMapper mapper, ILogger<ReviewService> logger,
        IMemoryCache memoryCache)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<ReviewReadDto?> CreateReviewAsync(ReviewWriteDto dto)
    {
        _logger.LogInformation(ReviewServiceLogMessages.CreateReviewRequestReceived);

        if (dto == null)
        {
            _logger.LogWarning(ReviewServiceLogMessages.InvalidReviewDataReceived);
            throw new InvalidReviewDataReceivedException(ReviewServiceCustomMessages.InvalidReviewDataReceived);
        }

        _logger.LogInformation(ReviewServiceLogMessages.CorrectReviewInformationSent);

        var review = _mapper.Map<Review>(dto);

        await _reviewRepository.AddAsync(review);

        _logger.LogInformation(ReviewServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(ReviewsCacheKey);
        _memoryCache.Remove(ReviewCacheKey);

        var reviewReadDto = _mapper.Map<ReviewReadDto>(review);
        return reviewReadDto;
    }

    public async Task<List<ReviewReadDto>?> GetReviewsAsync()
    {
        _logger.LogInformation(ReviewServiceLogMessages.FetchingReviewsFromRepository);

        if (_memoryCache.TryGetValue(ReviewsCacheKey, out List<ReviewReadDto> cachedReviews))
        {
            _logger.LogInformation(ReviewServiceLogMessages.ReturningReviewsFromCache);
            return cachedReviews;
        }

        var reviews = await _reviewRepository.GetAllAsync();
        if (reviews == null)
        {
            _logger.LogWarning(ReviewServiceCustomMessages.FailedFetchingReviewsFromRepository);
            throw new FailedToFetchReviewsException(ReviewServiceCustomMessages.FailedFetchingReviewsFromRepository);
        }

        _logger.LogInformation(ReviewServiceLogMessages.FetchedReviewsFromRepositorySuccessfully);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(Constants.AbsoluteExpirationForRetrieveReviewsMinutes))
            .SetSlidingExpiration(TimeSpan.FromMinutes(Constants.SlidingExpirationMinutes))
            .SetSize(Constants.CachingUnitSize);

        var reviewsReadDto = _mapper.Map<List<ReviewReadDto>>(reviews);

        _memoryCache.Set(ReviewsCacheKey, reviewsReadDto, cacheEntryOptions);
        return reviewsReadDto;
    }

    public async Task<ReviewReadDto?> GetReviewAsync(int reviewId)
    {
        _logger.LogInformation(ReviewServiceLogMessages.GetReviewRequestReceived, reviewId);

        if (_memoryCache.TryGetValue(ReviewCacheKey, out ReviewReadDto cachedReview))
        {
            _logger.LogInformation(ReviewServiceLogMessages.ReturningReviewFromCache);
            return cachedReview;
        }

        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null) return null;

        _logger.LogInformation(ReviewServiceLogMessages.FetchedReviewFromRepositorySuccessfully, reviewId);

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(Constants.AbsoluteExpirationForRetrieveReviewsMinutes))
            .SetSlidingExpiration(TimeSpan.FromMinutes(Constants.SlidingExpirationMinutes))
            .SetSize(Constants.CachingUnitSize);

        var reviewReadDto = _mapper.Map<ReviewReadDto>(review);

        _memoryCache.Set(ReviewCacheKey, reviewReadDto, cacheEntryOptions);
        return reviewReadDto;
    }

    public async Task<ReviewReadDto?> UpdateReviewAsync(int reviewId, ReviewPatchDto dto)
    {
        _logger.LogInformation(ReviewServiceLogMessages.UpdateReviewRequestReceived, reviewId);

        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null) return null;

        _logger.LogInformation(ReviewServiceLogMessages.RetrieveReviewSuccessfullyFromRepository, reviewId);

        review.Rating = dto.Rating ?? review.Rating;
        review.Comment = dto.Comment ?? review.Comment;
        review.UserId = dto.UserId ?? review.UserId;
        review.HotelId = dto.HotelId ?? review.HotelId;

        review.LastUpdated = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review);

        _logger.LogInformation(ReviewServiceLogMessages.DeleteCachedData);
        _memoryCache.Remove(ReviewsCacheKey);
        _memoryCache.Remove(ReviewCacheKey);

        var reviewReadDto = _mapper.Map<ReviewReadDto>(review);
        return reviewReadDto;
    }

    public async Task DeleteReviewAsync(int reviewId)
    {
        _logger.LogInformation(ReviewServiceLogMessages.DeleteReviewRequestReceived, reviewId);

        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null) return;
        _logger.LogInformation(ReviewServiceLogMessages.RetrieveReviewSuccessfullyFromRepository, reviewId);

        await _reviewRepository.DeleteAsync(review);
        _logger.LogInformation(ReviewServiceLogMessages.ReviewDeletedSuccessfully, reviewId);
    }
}