using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeAndContactPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Internships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "Grade", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b8c319de-a076-4a16-b265-c9eed64882ec", null, "AQAAAAIAAYagAAAAENbsBHC2QKGVmrx+xY1MfiTHm9ysnAZJLBr4pDiMyPa1eV2DcpYwhd9drWOSzZZ9HA==", "d8275fb3-e56a-4ac4-92ed-ef13cb558ffb" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Internships");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0c222918-b0df-445c-a3f6-693aebb79f51", "AQAAAAIAAYagAAAAEBfYi+mzWtbKdad5lYZfsMZ7CYgGUXwCIK5tO8Bf7Co+EUcVLQQcB+S4dJB4DLzV3A==", "39366918-f729-4c72-96c2-5ad920bc8e9c" });
        }
    }
}
