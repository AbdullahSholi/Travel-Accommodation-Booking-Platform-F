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
                    services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
                });

                builder.UseEnvironment("Testing");
            });

        Client = Factory.CreateClient();

        using var scope = Factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DbContext.Database.EnsureCreatedAsync();
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

    protected async Task ClearDatabaseAsync()
    {
        using var context = GetDbContext();
        context.Users.RemoveRange(context.Users);
        context.OtpRecords.RemoveRange(context.OtpRecords);
        await context.SaveChangesAsync();
    }
}