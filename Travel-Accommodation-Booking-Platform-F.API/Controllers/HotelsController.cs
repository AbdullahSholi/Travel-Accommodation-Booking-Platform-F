using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.HotelService;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.HotelExceptions;
using Travel_Accommodation_Booking_Platform_F.Utils.Hotel;
using CustomMessages = Travel_Accommodation_Booking_Platform_F.Utils.Hotel.CustomMessages;

namespace Travel_Accommodation_Booking_Platform_F.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class HotelsController : ControllerBase
{
    private readonly IHotelService _hotelService;
    private readonly ILogger<HotelsController> _logger;

    public HotelsController(IHotelService hotelService, ILogger<HotelsController> logger)
    {
        _hotelService = hotelService;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateHotel([FromBody] HotelWriteDto dto)
    {
        _logger.LogInformation(LogMessages.CreateHotelRequestReceived);
        try
        {
            var hotel = await _hotelService.CreateHotelAsync(dto);
            if (hotel == null)
            {
                _logger.LogWarning(LogMessages.CreateHotelFailed);
                return BadRequest(new { Message = CustomMessages.InvalidHotelCreationCredentials });
            }

            _logger.LogInformation(LogMessages.CreateHotelSuccess, hotel.HotelId);
            return CreatedAtAction(nameof(GetHotelById), new { Id = hotel.HotelId }, hotel);
        }
        catch (InvalidHotelDataReceivedException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.CreateHotelFailed);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User, Admin")]
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<IActionResult> GetHotels()
    {
        _logger.LogInformation(LogMessages.GetHotelsRequestReceived);

        try
        {
            var hotels = await _hotelService.GetHotelsAsync();
            if (hotels == null)
            {
                _logger.LogWarning(LogMessages.GetHotelsFailed);
                return NotFound(new { Message = CustomMessages.ListOfHotelsIsNotFound });
            }

            var lastUpdated = hotels.Max(u => u.LastUpdated);
            var eTag = $"\"{lastUpdated.Ticks}\"";
            
            _logger.LogInformation(LogMessages.CheckIfListOfHotelsNotUpdatedRecently);
            var clientETag = Request.Headers["If-None-Match"].FirstOrDefault();
            if (clientETag == eTag)
            {
                _logger.LogInformation(LogMessages.RetrievedDataFromBrowserCache);
                return StatusCode(StatusCodes.Status304NotModified);
            }
            
            _logger.LogInformation(LogMessages.SendETagToClientWhenListOfHotelsUpdatedRecently);
            Response.Headers["ETag"] = eTag;
            
            _logger.LogInformation(LogMessages.GetHotelsSuccess);
            return Ok(hotels);
        }
        catch (FailedToFetchHotelsException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.GetHotelsFailed);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User, Admin")]
    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<IActionResult> GetHotelById([FromRoute] int id)
    {
        _logger.LogInformation(LogMessages.GetHotelRequestReceived, id);

        try
        {
            var hotel = await _hotelService.GetHotelAsync(id);
            if (hotel == null)
            {
                _logger.LogWarning(LogMessages.GetHotelFailed, id);
                return NotFound(new { Message = CustomMessages.HotelNotFound });
            }
            
            var eTag = $"\"{hotel.LastUpdated.Ticks}\"";
            
            _logger.LogInformation(LogMessages.CheckIfHotelIsNotUpdatedRecently);
            var clientETag = Request.Headers["If-None-Match"].FirstOrDefault();
            if (!string.IsNullOrEmpty(clientETag) && clientETag == eTag)
            {
                _logger.LogInformation(LogMessages.RetrievedDataFromBrowserCache);
                return StatusCode(StatusCodes.Status304NotModified);
            }
            
            _logger.LogInformation(LogMessages.SendETagToClientWhenHotelUpdatedRecently);
            Response.Headers["ETag"] = eTag;

            _logger.LogInformation(LogMessages.GetHotelSuccess, id);
            return Ok(hotel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.GetHotelFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateHotel([FromRoute] int id, [FromBody] HotelPatchDto dto)
    {
        _logger.LogInformation(LogMessages.UpdateHotelRequestReceived, id);
        
        try
        {
            var hotel = await _hotelService.GetHotelAsync(id);
            if (hotel == null)
            {
                _logger.LogWarning(LogMessages.GetHotelFailed, id);
                return NotFound(new { Message = CustomMessages.HotelNotFound });
            }
            
            var currentETag = $"\"{hotel.LastUpdated.Ticks}\"";
            
            _logger.LogInformation(LogMessages.CheckIfUserTryUpdateTheLastVersionOfData);
            var clientETag = Request.Headers["If-Match"].FirstOrDefault();
            if (clientETag == null || clientETag != currentETag)
            {
                _logger.LogWarning(LogMessages.UserTryUpdateOldVersionOfData);
                return StatusCode(StatusCodes.Status412PreconditionFailed);
            }
            
            var updatedHotel = await _hotelService.UpdateHotelAsync(id, dto);
            if (updatedHotel == null)
            {
                _logger.LogWarning(LogMessages.UpdateHotelFailed, id);
                return NotFound(new { Message = CustomMessages.HotelNotFound });
            }

            _logger.LogInformation(LogMessages.HotelUpdatedSuccessfully, id);
            return Ok(updatedHotel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.UpdateHotelFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHotel([FromRoute] int id)
    {
        try
        {
            _logger.LogInformation(LogMessages.DeleteHotelRequestReceived, id);
            await _hotelService.DeleteHotelAsync(id);

            _logger.LogInformation(LogMessages.HotelUpdatedSuccessfully, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.DeleteHotelFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }
}