using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.ReviewService;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.ReviewExceptions;
using Travel_Accommodation_Booking_Platform_F.Utils.Review;
using CustomMessages = Travel_Accommodation_Booking_Platform_F.Utils.Review.CustomMessages;

namespace Travel_Accommodation_Booking_Platform_F.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    [Authorize(Roles = "User, Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] ReviewWriteDto dto)
    {
        _logger.LogInformation(LogMessages.CreateReviewRequestReceived);
        try
        {
            var review = await _reviewService.CreateReviewAsync(dto);
            if (review == null)
            {
                _logger.LogWarning(LogMessages.CreateReviewFailed);
                return BadRequest(new { Message = CustomMessages.InvalidReviewCreationCredentials });
            }

            _logger.LogInformation(LogMessages.CreateReviewSuccess, review.ReviewId);
            return CreatedAtAction(nameof(GetReviewById), new { Id = review.ReviewId }, review);
        }
        catch (InvalidReviewDataReceivedException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.CreateReviewFailed);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User, Admin")]
    [HttpGet]
    public async Task<IActionResult> GetReviews()
    {
        _logger.LogInformation(LogMessages.GetReviewsRequestReceived);

        try
        {
            var reviews = await _reviewService.GetReviewsAsync();
            if (reviews == null)
            {
                _logger.LogWarning(LogMessages.GetReviewsFailed);
                return NotFound(new { Message = CustomMessages.ListOfReviewsIsNotFound });
            }

            var lastUpdated = reviews.Max(u => u.LastUpdated);
            var eTag = $"\"{lastUpdated.Ticks}\"";

            _logger.LogInformation(LogMessages.CheckIfListOfReviewsNotUpdatedRecently);
            var clientETag = Request.Headers["If-None-Match"].FirstOrDefault();
            if (clientETag == eTag)
            {
                _logger.LogInformation(LogMessages.RetrievedDataFromBrowserCache);
                return StatusCode(StatusCodes.Status304NotModified);
            }

            _logger.LogInformation(LogMessages.SendETagToClientWhenListOfReviewsUpdatedRecently);
            Response.Headers["ETag"] = eTag;

            _logger.LogInformation(LogMessages.GetReviewsSuccess);
            return Ok(reviews);
        }
        catch (FailedToFetchReviewsException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.GetReviewsFailed);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User, Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReviewById([FromRoute] int id)
    {
        _logger.LogInformation(LogMessages.GetReviewRequestReceived, id);

        try
        {
            var review = await _reviewService.GetReviewAsync(id);
            if (review == null)
            {
                _logger.LogWarning(LogMessages.GetReviewFailed, id);
                return NotFound(new { Message = CustomMessages.ReviewNotFound });
            }

            var eTag = $"\"{review.LastUpdated.Ticks}\"";

            _logger.LogInformation(LogMessages.CheckIfReviewIsNotUpdatedRecently);
            var clientETag = Request.Headers["If-None-Match"].FirstOrDefault();
            if (!string.IsNullOrEmpty(clientETag) && clientETag == eTag)
            {
                _logger.LogInformation(LogMessages.RetrievedDataFromBrowserCache);
                return StatusCode(StatusCodes.Status304NotModified);
            }

            _logger.LogInformation(LogMessages.SendETagToClientWhenReviewUpdatedRecently);
            Response.Headers["ETag"] = eTag;

            _logger.LogInformation(LogMessages.GetReviewSuccess, id);
            return Ok(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.GetReviewFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User, Admin")]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateReview([FromRoute] int id, [FromBody] ReviewPatchDto dto)
    {
        _logger.LogInformation(LogMessages.UpdateReviewRequestReceived, id);

        try
        {
            var review = await _reviewService.GetReviewAsync(id);
            if (review == null)
            {
                _logger.LogWarning(LogMessages.GetReviewFailed, id);
                return NotFound(new { Message = CustomMessages.ReviewNotFound });
            }

            var currentETag = $"\"{review.LastUpdated.Ticks}\"";

            _logger.LogInformation(LogMessages.CheckIfUserTryUpdateTheLastVersionOfData);
            var clientETag = Request.Headers["If-Match"].FirstOrDefault();
            if (clientETag == null || clientETag != currentETag)
            {
                _logger.LogWarning(LogMessages.UserTryUpdateOldVersionOfData);
                return StatusCode(StatusCodes.Status412PreconditionFailed);
            }

            var updatedReview = await _reviewService.UpdateReviewAsync(id, dto);
            if (updatedReview == null)
            {
                _logger.LogWarning(LogMessages.UpdateReviewFailed, id);
                return NotFound(new { Message = CustomMessages.ReviewNotFound });
            }

            _logger.LogInformation(LogMessages.ReviewUpdatedSuccessfully, id);
            return Ok(updatedReview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.UpdateReviewFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User, Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReview([FromRoute] int id)
    {
        try
        {
            _logger.LogInformation(LogMessages.DeleteReviewRequestReceived, id);
            await _reviewService.DeleteReviewAsync(id);

            _logger.LogInformation(LogMessages.ReviewDeletedSuccessfully, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.DeleteReviewFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }
}