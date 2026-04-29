using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddEducationLevelToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EducationLevel",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "EducationLevel", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9bcd9db1-0c62-4fa7-b2c5-540e20664e0a", null, "AQAAAAIAAYagAAAAEHd5ooI7Ghv+a9XR6Q3lBZN/dxv1UvKDIfgFMui7GMMLVU9b/qPc4xiQboEzikzeqg==", "58dc1d64-f300-4467-9ff2-4cde8ffbb4ce" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationLevel",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4be7357a-7033-41fa-80e5-0afb01ef39ac", "AQAAAAIAAYagAAAAENw5sOXTCrfqmPEGpb+pmLI9TJFqWYdbnXaXKsO8M8OeXCP6t4RGPaizTjPT6oy5hA==", "316b32b9-5f43-447a-9a30-71473bea3b24" });
        }
    }
}
