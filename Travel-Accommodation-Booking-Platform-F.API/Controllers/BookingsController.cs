using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.BookingService;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.BookingExceptions;
using Travel_Accommodation_Booking_Platform_F.Utils.Booking;

namespace Travel_Accommodation_Booking_Platform_F.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [Authorize(Roles = "User, Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] BookingWriteDto dto)
    {
        _logger.LogInformation(LogMessages.CreateBookingRequestReceived);
        try
        {
            var booking = await _bookingService.CreateBookingAsync(dto);
            if (booking == null)
            {
                _logger.LogWarning(LogMessages.CreateBookingFailed);
                return BadRequest(new { Message = CustomMessages.InvalidBookingCreationCredentials });
            }

            _logger.LogInformation(LogMessages.CreateBookingSuccess, booking.BookingId);
            return CreatedAtAction(nameof(GetBookingById), new { Id = booking.BookingId }, booking);
        }
        catch (InvalidBookingDataReceivedException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.CreateBookingFailed);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetBookings()
    {
        _logger.LogInformation(LogMessages.GetBookingsRequestReceived);

        try
        {
            var bookings = await _bookingService.GetBookingsAsync();
            if (bookings == null)
            {
                _logger.LogWarning(LogMessages.GetBookingsFailed);
                return NotFound(new { Message = CustomMessages.ListOfBookingsIsNotFound });
            }

            var lastUpdated = bookings.Max(u => u.LastUpdated);
            var eTag = $"\"{lastUpdated.Ticks}\"";

            _logger.LogInformation(LogMessages.CheckIfListOfBookingsNotUpdatedRecently);
            var clientETag = Request.Headers["If-None-Match"].FirstOrDefault();
            if (clientETag == eTag)
            {
                _logger.LogInformation(LogMessages.RetrievedDataFromBrowserCache);
                return StatusCode(StatusCodes.Status304NotModified);
            }

            _logger.LogInformation(LogMessages.SendETagToClientWhenListOfBookingsUpdatedRecently);
            Response.Headers["ETag"] = eTag;

            _logger.LogInformation(LogMessages.GetBookingsSuccess);
            return Ok(bookings);
        }
        catch (FailedToFetchBookingsException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.GetBookingsFailed);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User, Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBookingById([FromRoute] int id)
    {
        _logger.LogInformation(LogMessages.GetBookingRequestReceived, id);

        try
        {
            var booking = await _bookingService.GetBookingAsync(id);
            if (booking == null)
            {
                _logger.LogWarning(LogMessages.GetBookingFailed, id);
                return NotFound(new { Message = CustomMessages.BookingNotFound });
            }

            var eTag = $"\"{booking.LastUpdated.Ticks}\"";

            _logger.LogInformation(LogMessages.CheckIfBookingIsNotUpdatedRecently);
            var clientETag = Request.Headers["If-None-Match"].FirstOrDefault();
            if (!string.IsNullOrEmpty(clientETag) && clientETag == eTag)
            {
                _logger.LogInformation(LogMessages.RetrievedDataFromBrowserCache);
                return StatusCode(StatusCodes.Status304NotModified);
            }

            _logger.LogInformation(LogMessages.SendETagToClientWhenBookingUpdatedRecently);
            Response.Headers["ETag"] = eTag;

            _logger.LogInformation(LogMessages.GetBookingSuccess, id);
            return Ok(booking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.GetBookingFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateBooking([FromRoute] int id, [FromBody] BookingPatchDto dto)
    {
        _logger.LogInformation(LogMessages.UpdateBookingRequestReceived, id);

        try
        {
            var booking = await _bookingService.GetBookingAsync(id);
            if (booking == null)
            {
                _logger.LogWarning(LogMessages.GetBookingFailed, id);
                return NotFound(new { Message = CustomMessages.BookingNotFound });
            }

            var currentETag = $"\"{booking.LastUpdated.Ticks}\"";

            _logger.LogInformation(LogMessages.CheckIfUserTryUpdateTheLastVersionOfData);
            var clientETag = Request.Headers["If-Match"].FirstOrDefault();
            if (clientETag == null || clientETag != currentETag)
            {
                _logger.LogWarning(LogMessages.UserTryUpdateOldVersionOfData);
                return StatusCode(StatusCodes.Status412PreconditionFailed);
            }

            var updatedBooking = await _bookingService.UpdateBookingAsync(id, dto);
            if (updatedBooking == null)
            {
                _logger.LogWarning(LogMessages.UpdateBookingFailed, id);
                return NotFound(new { Message = CustomMessages.BookingNotFound });
            }

            _logger.LogInformation(LogMessages.BookingUpdatedSuccessfully, id);
            return Ok(updatedBooking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.UpdateBookingFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBooking([FromRoute] int id)
    {
        try
        {
            _logger.LogInformation(LogMessages.DeleteBookingRequestReceived, id);
            await _bookingService.DeleteBookingAsync(id);

            _logger.LogInformation(LogMessages.BookingDeletedSuccessfully, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.DeleteBookingFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }
}