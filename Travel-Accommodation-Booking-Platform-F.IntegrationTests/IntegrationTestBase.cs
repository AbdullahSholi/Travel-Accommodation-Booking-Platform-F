using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;
using Xunit;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer;
    protected WebApplicationFactory<Program> Factory;
    protected HttpClient Client;
    protected ApplicationDbContext DbContext { get; private set; }

    protected IntegrationTestBase()
    {
        _sqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Sholi@971")
            .WithCleanUp(true)
            .Build();
    }

    public virtual async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                    services.RemoveAll(typeof(ApplicationDbContext));

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlServer(_sqlContainer.GetConnectionString());
                    });

                    services.AddMemoryCache();
                    services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
                });

                builder.UseEnvironment("Testing");
            });

        Client = Factory.CreateClient();

        using var scope = Factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
        await Factory.DisposeAsync();
        Client.Dispose();
    }

    protected ApplicationDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    protected async Task SeedUsersAsync(params User[] users)
    {
        using var context = GetDbContext();
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    protected async Task SeedOtpRecordsAsync(params OtpRecord[] otpRecords)
    {
        using var context = GetDbContext();
        context.OtpRecords.AddRange(otpRecords);
        await context.SaveChangesAsync();
    }

    protected async Task SeedBlacklistedTokensAsync(params BlacklistedToken[] blacklistedTokens)
    {
        using var context = GetDbContext();
        context.BlacklistedTokens.AddRange(blacklistedTokens);
        await context.SaveChangesAsync();
    }

    protected async Task SeedCitiesAsync(params City[] cities)
    {
        using var context = GetDbContext();
        foreach (var city in cities)
        {
            var cityEntity = new City
            {
                Name = city.Name,
                Country = city.Country,
                PostOffice = city.PostOffice,
                NumberOfHotels = city.NumberOfHotels,
                CreatedAt = city.CreatedAt,
                UpdatedAt = city.UpdatedAt,
                LastUpdated = city.LastUpdated
            };
            context.Cities.Add(cityEntity);
        }

        await context.SaveChangesAsync();
    }

    protected async Task SeedRoomsAsync(params Room[] rooms)
    {
        using var context = GetDbContext();
        foreach (var room in rooms)
        {
            var roomEntity = new Room
            {
                RoomType = room.RoomType,
                Images = room.Images,
                Description = room.Description,
                PricePerNight = room.PricePerNight,
                IsAvailable = room.IsAvailable,
                AdultCapacity = room.AdultCapacity,
                ChildrenCapacity = room.ChildrenCapacity,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt,
                HotelId = room.HotelId,
                LastUpdated = room.LastUpdated
            };
            context.Rooms.Add(roomEntity);
        }

        await context.SaveChangesAsync();
    }

    protected async Task SeedReviewsAsync(params Review[] reviews)
    {
        using var context = GetDbContext();
        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();
    }

    protected async Task SeedHotelsAsync(params Hotel[] hotels)
    {
        // We do this to enforce database to make the hotelId auto increment
        using var context = GetDbContext();
        foreach (var hotel in hotels)
        {
            var hotelEntity = new Hotel
            {
                HotelName = hotel.HotelName,
                OwnerName = hotel.OwnerName,
                StarRating = hotel.StarRating,
                Location = hotel.Location,
                Description = hotel.Description,
                CityId = hotel.CityId,
                LastUpdated = hotel.LastUpdated
            };
            context.Hotels.Add(hotelEntity);
        }

        await context.SaveChangesAsync();
    }


    protected async Task ClearDatabaseAsync()
    {
        using var context = GetDbContext();
        context.OtpRecords.RemoveRange(context.OtpRecords);
        context.Bookings.RemoveRange(context.Bookings);
        context.Reviews.RemoveRange(context.Reviews);
        context.Rooms.RemoveRange(context.Rooms);
        context.Hotels.RemoveRange(context.Hotels);
        context.Users.RemoveRange(context.Users);
        context.Cities.RemoveRange(context.Cities);
        context.BlacklistedTokens.RemoveRange(context.BlacklistedTokens);
        await context.SaveChangesAsync();
    }
}