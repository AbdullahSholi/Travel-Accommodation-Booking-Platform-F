using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Accommodation_Booking_Platform_F.Application.DTOs.WriteDTOs;
using Travel_Accommodation_Booking_Platform_F.Application.Services.CityService;
using Travel_Accommodation_Booking_Platform_F.Domain.CustomExceptions.CityExceptions;
using Travel_Accommodation_Booking_Platform_F.Utils.City;
using CustomMessages = Travel_Accommodation_Booking_Platform_F.Utils.City.CustomMessages;

namespace Travel_Accommodation_Booking_Platform_F.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CitiesController : ControllerBase
{
    private readonly ICityService _cityService;
    private readonly ILogger<CitiesController> _logger;

    public CitiesController(ICityService cityService, ILogger<CitiesController> logger)
    {
        _cityService = cityService;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCity([FromBody] CityWriteDto dto)
    {
        _logger.LogInformation(LogMessages.CreateCityRequestReceived);
        try
        {
            var city = await _cityService.CreateCityAsync(dto);
            if (city == null)
            {
                _logger.LogWarning(LogMessages.CreateCityFailed);
                return BadRequest(new { Message = CustomMessages.InvalidCityCreationCredentials });
            }

            _logger.LogInformation(LogMessages.CreateCitySuccess, city.CityId);
            return CreatedAtAction(nameof(GetCityById), new { Id = city.CityId }, city);
        }
        catch (InvalidCityDataReceivedException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.CreateCityFailed);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User, Admin")]
    [HttpGet]
    public async Task<IActionResult> GetCities()
    {
        _logger.LogInformation(LogMessages.GetCitiesRequestReceived);

        try
        {
            var cities = await _cityService.GetCitiesAsync();
            if (cities == null)
            {
                _logger.LogWarning(LogMessages.GetCitiesFailed);
                return NotFound(new { Message = CustomMessages.ListOfCitiesIsNotFound });
            }

            _logger.LogInformation(LogMessages.GetCitiesSuccess);
            return Ok(cities);
        }
        catch (FailedToFetchCitiesException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.GetCitiesFailed);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "User, Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCityById([FromRoute] int id)
    {
        _logger.LogInformation(LogMessages.GetCityRequestReceived, id);

        try
        {
            var city = await _cityService.GetCityAsync(id);
            if (city == null)
            {
                _logger.LogWarning(LogMessages.GetCityFailed, id);
                return NotFound(new { Message = CustomMessages.CityNotFound });
            }

            _logger.LogInformation(LogMessages.GetCitySuccess, id);
            return Ok(city);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.GetCityFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateCity([FromRoute] int id, [FromBody] CityPatchDto dto)
    {
        try
        {
            _logger.LogInformation(LogMessages.UpdateCityRequestReceived, id);
            var updatedCity = await _cityService.UpdateCityAsync(id, dto);
            if (updatedCity == null)
            {
                _logger.LogWarning(LogMessages.UpdateCityFailed, id);
                return NotFound(new { Message = CustomMessages.CityNotFound });
            }

            _logger.LogInformation(LogMessages.CityUpdatedSuccessfully, id);
            return Ok(updatedCity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.UpdateCityFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCity([FromRoute] int id)
    {
        try
        {
            _logger.LogInformation(LogMessages.DeleteCityRequestReceived, id);
            await _cityService.DeleteCityAsync(id);

            _logger.LogInformation(LogMessages.CityUpdatedSuccessfully, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogMessages.DeleteCityFailed, id);
            return StatusCode(500, new { Message = CustomMessages.InternalServerError });
        }
    }
}