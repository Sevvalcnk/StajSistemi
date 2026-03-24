using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddCareerFieldsToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CVPath",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificatePath",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducationSummary",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalSkills",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "d8f81c47-c9f9-4c60-801f-45852a15af67", "AQAAAAIAAYagAAAAEGkVkVA82IYKfe2CGQwI25BhSQ57T9qY/9Yo43Fu8+kcFx8yh9qtgO6X3uE+v08dIQ==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVPath",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CertificatePath",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EducationSummary",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PersonalSkills",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Students");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "65dc21b0-2318-4e65-9e6a-9a683384a40b", "AQAAAAIAAYagAAAAEHKLhMqZIwRKnSVzQ+tcRaHyYTX8BqmMZPhvgMuH8aOxQYiRPiXiY6JWTLh2hFH9ug==" });
        }
    }
}
