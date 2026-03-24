using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class FixInternshipIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5b889baf-394c-4e18-8833-a49830d44da3", "AQAAAAIAAYagAAAAEOCNxUj8wYovmYZ4qi//lOVow6bXGwneFR2dXIgVMQC5wGvu17zYyk9WybqcRBVw1A==", "a944bce7-7509-485c-89db-670973cb7f7f" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f2b4d32b-be6f-436c-9fe5-719d110935f8", "AQAAAAIAAYagAAAAEDH6dUQ9rCR4sDW9tyDEfdw5zoZjGEJmwluFWNrGu9k0rm/HI9rVyXc8ts+yxMjkLw==", "48a149db-848e-4d73-82a6-87550c1780ec" });
        }
    }
}
