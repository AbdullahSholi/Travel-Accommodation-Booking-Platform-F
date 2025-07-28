using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel_Accommodation_Booking_Platform_F.Domain.Configurations;

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Persistence.Configurations;

public class OtpRecordConfiguration : IEntityTypeConfiguration<OtpRecord>
{
    public void Configure(EntityTypeBuilder<OtpRecord> builder)
    {
        builder.Property(e => e.Expiration)
            .HasConversion(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    }
}