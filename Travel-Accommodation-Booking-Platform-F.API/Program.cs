using Travel_Accommodation_Booking_Platform_F.Application.Services;
using Travel_Accommodation_Booking_Platform_F.Domain.Repositories;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();


var app = builder.Build();

app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:80");

app.MapGet("/", () => "Hello from Travel Accommodation API! - Activate CI/CD Pipeline");

app.Run();