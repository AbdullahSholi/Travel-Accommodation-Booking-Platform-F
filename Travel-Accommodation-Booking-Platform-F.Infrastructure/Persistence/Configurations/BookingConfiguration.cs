using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;
using Travel_Accommodation_Booking_Platform_F.Domain.Entities;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(b => b.TotalPrice)
            .HasPrecision(18, 2);
    }
}