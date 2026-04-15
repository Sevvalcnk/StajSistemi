using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class TurkiyeGeneliGuncelleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Application");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0f6ab706-78c2-41b7-ac17-e885f010450c", "AQAAAAIAAYagAAAAEOuJUQYrD5S9yOWo6jQlBpysmkt2Wb3x3V1q6METVeCpofqtdqVubw7/D9GecqMKtA==", "55e380af-c14b-4563-8d67-a5a5c3ab72fa" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Application",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<int>(type: "int", nullable: false),
                    InternshipId = table.Column<int>(type: "int", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b13ec2ad-43f7-43c2-8ef3-16b2be7cfcea", "AQAAAAIAAYagAAAAEG/rJaxugqT10M9NTrC5RLIkSy2YIZAUvtItYhjKt6lPir0OR03nfKxka1csg6idzw==", "1e321f79-69f7-4a91-9b26-69c6ae98e4f0" });

            migrationBuilder.CreateIndex(
                name: "IX_Application_AppUserId",
                table: "Application",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Application_InternshipId",
                table: "Application",
                column: "InternshipId");
        }
    }
}
