using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class FinalTypeFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0997cf7a-4dd3-44a8-b3dd-bf59f0a7e0e0", "AQAAAAIAAYagAAAAEJuNidb9j4rCGtJLugQsd5rYVVX1SfNQMApHaowDLmyE2m3yiVeXYJpuOHN1TrlOcw==", "f612d512-007f-465b-8f1a-fb7c6c6b85cd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5b889baf-394c-4e18-8833-a49830d44da3", "AQAAAAIAAYagAAAAEOCNxUj8wYovmYZ4qi//lOVow6bXGwneFR2dXIgVMQC5wGvu17zYyk9WybqcRBVw1A==", "a944bce7-7509-485c-89db-670973cb7f7f" });
        }
    }
}
