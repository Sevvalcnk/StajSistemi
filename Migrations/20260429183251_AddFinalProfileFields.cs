using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d34092f0-1740-4118-82d7-acd7283bec45", "AQAAAAIAAYagAAAAEFQki7lnNEyKU3/gg4PJc7RQ8jabweSnWC/oarbAluHVT/rdrAk25xKFN3X40DYw9Q==", "9067fac3-550c-415f-9e1f-8b75623fcfd0" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "410b1607-d816-4c5a-b416-72d83f399bf0", "AQAAAAIAAYagAAAAEIbC2sE8VrPoP6iWyYNLlHT2PfpkF8BWpvJ1JcUW/jKqYTVcdtBgiHE265JwHllB3g==", "7eba502d-9e07-4958-a49e-26c687a5867c" });
        }
    }
}
