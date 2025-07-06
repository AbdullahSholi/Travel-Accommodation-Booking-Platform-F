using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.RoomService;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.RoomExceptions;
using Travel_Accommodation_Booking_Platform_F.Utils.Room;
using CustomMessages = Travel_Accommodation_Booking_Platform_F.Utils.Room.CustomMessages;

namespace Travel_Accommodation_Booking_Platform_F.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(IRoomService roomService, ILogger<RoomsController> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] RoomWriteDto dto)
    {
        _logger.LogInformation(LogMessages.CreateRoomRequestReceived);
        try
        {
            var room = await _roomService.CreateRoomAsync(dto);
            if (room == null)
            {
                _logger.LogWarning(LogMessages.CreateRoomFailed);
                return BadRequest(new { Message = CustomMessages.InvalidRoomCreationCredentials });
            }

            _logger.LogInformation(LogMessages.CreateRoomSuccess, room.RoomId);
            return CreatedAtAction(nameof(GetRoomById), new { Id = room.RoomId }, room);
        }
        catch (InvalidRoomDataReceivedException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.CreateRoomFailed);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User, Admin")]
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<IActionResult> GetRooms()
    {
        _logger.LogInformation(LogMessages.GetRoomsRequestReceived);

        try
        {
            var rooms = await _roomService.GetRoomsAsync();
            if (rooms == null)
            {
                _logger.LogWarning(LogMessages.GetRoomsFailed);
                return NotFound(new { Message = CustomMessages.ListOfRoomsIsNotFound });
            }
            
            var lastUpdated = rooms.Max(u => u.LastUpdated);
            var eTag = $"\"{lastUpdated.Ticks}\"";
            
            _logger.LogInformation(LogMessages.CheckIfListOfRoomsNotUpdatedRecently);
            var clientETag = Request.Headers["If-None-Match"].FirstOrDefault();
            if (clientETag == eTag)
            {
                _logger.LogInformation(LogMessages.RetrievedDataFromBrowserCache);
                return StatusCode(StatusCodes.Status304NotModified);
            }
            
            _logger.LogInformation(LogMessages.SendETagToClientWhenListOfRoomsUpdatedRecently);
            Response.Headers["ETag"] = eTag;

            _logger.LogInformation(LogMessages.GetRoomsSuccess);
            return Ok(rooms);
        }
        catch (FailedToFetchRoomsException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.GetRoomsFailed);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User, Admin")]
    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<IActionResult> GetRoomById([FromRoute] int id)
    {
        _logger.LogInformation(LogMessages.GetRoomRequestReceived, id);

        try
        {
            var room = await _roomService.GetRoomAsync(id);
            if (room == null)
            {
                _logger.LogWarning(LogMessages.GetRoomFailed, id);
                return NotFound(new { Message = CustomMessages.RoomNotFound });
            }
            
            var eTag = $"\"{room.LastUpdated.Ticks}\"";
            
            _logger.LogInformation(LogMessages.CheckIfRoomIsNotUpdatedRecently);
            var clientETag = Request.Headers["If-None-Match"].FirstOrDefault();
            if (!string.IsNullOrEmpty(clientETag) && clientETag == eTag)
            {
                _logger.LogInformation(LogMessages.RetrievedDataFromBrowserCache);
                return StatusCode(StatusCodes.Status304NotModified);
            }
            
            _logger.LogInformation(LogMessages.SendETagToClientWhenRoomUpdatedRecently);
            Response.Headers["ETag"] = eTag;

            _logger.LogInformation(LogMessages.GetRoomSuccess, id);
            return Ok(room);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.GetRoomFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateRoom([FromRoute] int id, [FromBody] RoomPatchDto dto)
    {
        _logger.LogInformation(LogMessages.UpdateRoomRequestReceived, id);
        
        try
        {
            var room = await _roomService.GetRoomAsync(id);
            if (room == null)
            {
                _logger.LogWarning(LogMessages.GetRoomFailed, id);
                return NotFound(new { Message = CustomMessages.RoomNotFound });
            }
            
            var currentETag = $"\"{room.LastUpdated.Ticks}\"";
            
            _logger.LogInformation(LogMessages.CheckIfUserTryUpdateTheLastVersionOfData);
            var clientETag = Request.Headers["If-Match"].FirstOrDefault();
            if (clientETag == null || clientETag != currentETag)
            {
                _logger.LogWarning(LogMessages.UserTryUpdateOldVersionOfData);
                return StatusCode(StatusCodes.Status412PreconditionFailed);
            }
            
            var updatedRoom = await _roomService.UpdateRoomAsync(id, dto);
            if (updatedRoom == null)
            {
                _logger.LogWarning(LogMessages.UpdateRoomFailed, id);
                return NotFound(new { Message = CustomMessages.RoomNotFound });
            }

            _logger.LogInformation(LogMessages.RoomUpdatedSuccessfully, id);
            return Ok(updatedRoom);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.UpdateRoomFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRoom([FromRoute] int id)
    {
        try
        {
            _logger.LogInformation(LogMessages.DeleteRoomRequestReceived, id);
            await _roomService.DeleteRoomAsync(id);

            _logger.LogInformation(LogMessages.RoomUpdatedSuccessfully, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.DeleteRoomFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }
}