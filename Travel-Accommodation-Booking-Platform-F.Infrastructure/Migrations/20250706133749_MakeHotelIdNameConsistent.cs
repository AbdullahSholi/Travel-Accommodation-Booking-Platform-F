using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeHotelIdNameConsistent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Hotels",
                newName: "HotelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "Hotels",
                newName: "Id");
        }
    }
}
