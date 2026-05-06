using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class FinalVersionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f78bf411-9268-440f-8aa6-5f31fde6b45e", "AQAAAAIAAYagAAAAEEF8c45tiCHkYfljw488r2ZJnXNk65Sdw2UDzmgO8EMwh4BuLhm6ZBkkHsLnQDeEBQ==", "5d88c17e-fb88-4c7d-9263-56f23e8cde72" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "12d82e4a-828c-45e5-ab9e-9cb91cf32f5d", "AQAAAAIAAYagAAAAEHvDJE6ZDSxS2nRsGwsHQBYmZDvDN2SnGOjavxYaee+TiuQVr5m8tVTLBhcVJKGxeA==", "4dd444b1-8854-4a33-8c86-39f068285c76" });
        }
    }
}
