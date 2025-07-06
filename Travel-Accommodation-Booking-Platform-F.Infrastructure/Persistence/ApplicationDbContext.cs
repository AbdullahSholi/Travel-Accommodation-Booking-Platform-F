using Microsoft.EntityFrameworkCore;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;
using Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence.Configurations;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<OtpRecord> OtpRecords => Set<OtpRecord>();
    public DbSet<BlacklistedToken> BlacklistedTokens => Set<BlacklistedToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OtpRecordConfiguration());
        modelBuilder.ApplyConfiguration(new BookingConfiguration());
        modelBuilder.ApplyConfiguration(new RoomConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}