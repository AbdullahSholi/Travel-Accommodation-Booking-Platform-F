using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Travel_Accommodation_Booking_Platform_F.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOneToManyRelationshipBetweenUserAndOtpRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "OtpRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OtpRecords_UserId",
                table: "OtpRecords",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OtpRecords_Users_UserId",
                table: "OtpRecords",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OtpRecords_Users_UserId",
                table: "OtpRecords");

            migrationBuilder.DropIndex(
                name: "IX_OtpRecords_UserId",
                table: "OtpRecords");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OtpRecords");
        }
    }
}
