using Microsoft.AspNetCore.Mvc;
using Travel_Accommodation_Booking_Platform_F.Application.Services;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;

namespace Travel_Accommodation_Booking_Platform_F.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : Controller
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<IEnumerable<User>> Get()
    {
        var products = _service.GetUsers();
        return Ok(products);
    }
}