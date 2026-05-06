using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class SyncProfileAndDepartments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "12d82e4a-828c-45e5-ab9e-9cb91cf32f5d", "AQAAAAIAAYagAAAAEHvDJE6ZDSxS2nRsGwsHQBYmZDvDN2SnGOjavxYaee+TiuQVr5m8tVTLBhcVJKGxeA==", "4dd444b1-8854-4a33-8c86-39f068285c76" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d34092f0-1740-4118-82d7-acd7283bec45", "AQAAAAIAAYagAAAAEFQki7lnNEyKU3/gg4PJc7RQ8jabweSnWC/oarbAluHVT/rdrAk25xKFN3X40DYw9Q==", "9067fac3-550c-415f-9e1f-8b75623fcfd0" });
        }
    }
}
