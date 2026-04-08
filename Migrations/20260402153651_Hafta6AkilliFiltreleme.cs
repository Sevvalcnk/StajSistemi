using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Hafta6AkilliFiltreleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "City",
                table: "Internships",
                newName: "CityName");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Internships",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinGPA",
                table: "Internships",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Application",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppUserId = table.Column<int>(type: "int", nullable: false),
                    InternshipId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Application", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Application_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Application_Internships_InternshipId",
                        column: x => x.InternshipId,
                        principalTable: "Internships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CityId", "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { null, "870885c5-4a16-493f-8e8f-f3fa1a392684", "AQAAAAIAAYagAAAAEGesTntkJ0CFzoGRzhsfTbsqk9N71zWyYhxpAGM3OEfr6+GAdjprTWN71sLO/D6tww==", "4ee6b7b3-31e7-4d8f-ab7f-3301b6907120" });

            migrationBuilder.CreateIndex(
                name: "IX_Internships_CityId",
                table: "Internships",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CityId",
                table: "AspNetUsers",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Application_AppUserId",
                table: "Application",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Application_InternshipId",
                table: "Application",
                column: "InternshipId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_City_CityId",
                table: "AspNetUsers",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Internships_City_CityId",
                table: "Internships",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_City_CityId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Internships_City_CityId",
                table: "Internships");

            migrationBuilder.DropTable(
                name: "Application");

            migrationBuilder.DropTable(
                name: "City");

            migrationBuilder.DropIndex(
                name: "IX_Internships_CityId",
                table: "Internships");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CityId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Internships");

            migrationBuilder.DropColumn(
                name: "MinGPA",
                table: "Internships");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "CityName",
                table: "Internships",
                newName: "City");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0997cf7a-4dd3-44a8-b3dd-bf59f0a7e0e0", "AQAAAAIAAYagAAAAEJuNidb9j4rCGtJLugQsd5rYVVX1SfNQMApHaowDLmyE2m3yiVeXYJpuOHN1TrlOcw==", "f612d512-007f-465b-8f1a-fb7c6c6b85cd" });
        }
    }
}
