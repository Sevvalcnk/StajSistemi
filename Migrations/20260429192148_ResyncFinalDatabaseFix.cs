using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class ResyncFinalDatabaseFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9ad40b7d-929a-4b3f-9ab0-f577f9d87c78", "AQAAAAIAAYagAAAAEPuPT4hDcyt0GrZuobrlQLLhgHWAH2jXU1cgZNtwXF3GHjg7M2bhY35Y6IvOfLhszw==", "f8c912f0-cf10-4937-8e66-15ccb61c2d5e" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a83d1c73-1e74-42f7-b2a1-cfc033e2569f", "AQAAAAIAAYagAAAAEO/WNYyh93u6O/qRCcyCEJ+K8acGImiVwLN8XCl8jokApWTWKo2MBPT20vBFKoQq0Q==", "ce2bfa87-96ac-4c18-804b-251ea13cc221" });
        }
    }
}
