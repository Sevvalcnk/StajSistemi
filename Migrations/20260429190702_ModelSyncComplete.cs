using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class ModelSyncComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a83d1c73-1e74-42f7-b2a1-cfc033e2569f", "AQAAAAIAAYagAAAAEO/WNYyh93u6O/qRCcyCEJ+K8acGImiVwLN8XCl8jokApWTWKo2MBPT20vBFKoQq0Q==", "ce2bfa87-96ac-4c18-804b-251ea13cc221" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9b23c39c-6f55-4383-bec0-0b72b07e7e7b", "AQAAAAIAAYagAAAAEOYtTv0ZRDIqh0vrzSKSvsI0gKGjftJjP0kRd2TiumneTxlZjKZhmGu/m0+2W7G7Xg==", "ad98b275-9d59-4be8-b358-2b0368ffc57b" });
        }
    }
}
