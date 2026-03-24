using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class RebuildInternshipApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipApplications_Departments_DepartmentId",
                table: "InternshipApplications");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "InternshipApplications");

            migrationBuilder.DropColumn(
                name: "CompanySector",
                table: "InternshipApplications");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "InternshipApplications",
                newName: "InternshipId");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "InternshipApplications",
                newName: "ApplicationDate");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipApplications_DepartmentId",
                table: "InternshipApplications",
                newName: "IX_InternshipApplications_InternshipId");

            migrationBuilder.AddColumn<string>(
                name: "StudentIP",
                table: "InternshipApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "65dc21b0-2318-4e65-9e6a-9a683384a40b", "AQAAAAIAAYagAAAAEHKLhMqZIwRKnSVzQ+tcRaHyYTX8BqmMZPhvgMuH8aOxQYiRPiXiY6JWTLh2hFH9ug==" });

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipApplications_Internships_InternshipId",
                table: "InternshipApplications",
                column: "InternshipId",
                principalTable: "Internships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipApplications_Internships_InternshipId",
                table: "InternshipApplications");

            migrationBuilder.DropColumn(
                name: "StudentIP",
                table: "InternshipApplications");

            migrationBuilder.RenameColumn(
                name: "InternshipId",
                table: "InternshipApplications",
                newName: "DepartmentId");

            migrationBuilder.RenameColumn(
                name: "ApplicationDate",
                table: "InternshipApplications",
                newName: "CreatedDate");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipApplications_InternshipId",
                table: "InternshipApplications",
                newName: "IX_InternshipApplications_DepartmentId");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "InternshipApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanySector",
                table: "InternshipApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "67f1b62f-1fd9-44fa-9f74-81d524ec28e1", "AQAAAAIAAYagAAAAEAm0u5fr4oNg8K1iP19KN5PBFCBgBBp7/HssVTHWx89T1YKbPaL+o4LX7ZtggvQwrA==" });

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipApplications_Departments_DepartmentId",
                table: "InternshipApplications",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
