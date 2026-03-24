using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileFieldsToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CVPath",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificatePath",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducationSummary",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GPA",
                table: "AspNetUsers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalSkills",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CVPath", "CertificatePath", "ConcurrencyStamp", "EducationSummary", "GPA", "PasswordHash", "PersonalSkills" },
                values: new object[] { null, null, "231f8561-eabd-42ca-8a92-7852fa74bc32", null, null, "AQAAAAIAAYagAAAAEI9Rx7wNJcqk/aTkKX4XdMGQKpdN4X2FH75Xd9IsfkO6v7L1+G7oyIu8vbpst5Uelg==", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVPath",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CertificatePath",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EducationSummary",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GPA",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PersonalSkills",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "577fdef1-2d08-4b89-91d5-b38cf4474206", "AQAAAAIAAYagAAAAEIj+5JyGXeOBm25IoE+pgc7OnxMf8YXnRZ4Xi8ZYV2DqEEBqSgQ3S0DrqxvgrFcOdA==" });
        }
    }
}
