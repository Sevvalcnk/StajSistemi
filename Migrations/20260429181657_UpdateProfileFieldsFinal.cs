using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProfileFieldsFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "410b1607-d816-4c5a-b416-72d83f399bf0", "AQAAAAIAAYagAAAAEIbC2sE8VrPoP6iWyYNLlHT2PfpkF8BWpvJ1JcUW/jKqYTVcdtBgiHE265JwHllB3g==", "7eba502d-9e07-4958-a49e-26c687a5867c" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2e48f89d-4b93-4b79-a0ff-a50d9ec7d20e", "AQAAAAIAAYagAAAAEMycICVgW2pRKsUywj4hX4kPTJCjtnePZn8KSkBihZYsgQGKF30Va07H/f5eCzlxBA==", "070655bf-45ab-472b-a1f1-95b668d6e5fe" });
        }
    }
}
