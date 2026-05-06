using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class FinalDegreeLevelSyncFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "99969980-9dbe-4731-b983-8528d54b5df8", "AQAAAAIAAYagAAAAELG2lVfeQVSmnTyuASUAhvq7YQ7oeH4cyXMH8c0YYmCW7zyGpsRxehHjOybj4wHSXQ==", "eb9e42f4-0395-4e5a-8783-ebc8155b444d" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9ad40b7d-929a-4b3f-9ab0-f577f9d87c78", "AQAAAAIAAYagAAAAEPuPT4hDcyt0GrZuobrlQLLhgHWAH2jXU1cgZNtwXF3GHjg7M2bhY35Y6IvOfLhszw==", "f8c912f0-cf10-4937-8e66-15ccb61c2d5e" });
        }
    }
}
