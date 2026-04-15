using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddDegreeTypeToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DegreeType",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "DegreeType", "PasswordHash", "SecurityStamp" },
                values: new object[] { "61378acf-0813-4748-a024-5a7136d9d8d0", null, "AQAAAAIAAYagAAAAEIHo8ryyAap6cWZ1sP3/Z7fOjZLtcNrUuCUQ+/PqrbtC0CjxZj1vrE3pFNCYQSe5mQ==", "81f31787-2b79-46ca-836c-1bf48612d8fd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DegreeType",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0f6ab706-78c2-41b7-ac17-e885f010450c", "AQAAAAIAAYagAAAAEOuJUQYrD5S9yOWo6jQlBpysmkt2Wb3x3V1q6METVeCpofqtdqVubw7/D9GecqMKtA==", "55e380af-c14b-4563-8d67-a5a5c3ab72fa" });
        }
    }
}
