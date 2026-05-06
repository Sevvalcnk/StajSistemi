using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class FinalDatabaseFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9b23c39c-6f55-4383-bec0-0b72b07e7e7b", "AQAAAAIAAYagAAAAEOYtTv0ZRDIqh0vrzSKSvsI0gKGjftJjP0kRd2TiumneTxlZjKZhmGu/m0+2W7G7Xg==", "ad98b275-9d59-4be8-b358-2b0368ffc57b" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f78bf411-9268-440f-8aa6-5f31fde6b45e", "AQAAAAIAAYagAAAAEEF8c45tiCHkYfljw488r2ZJnXNk65Sdw2UDzmgO8EMwh4BuLhm6ZBkkHsLnQDeEBQ==", "5d88c17e-fb88-4c7d-9263-56f23e8cde72" });
        }
    }
}
