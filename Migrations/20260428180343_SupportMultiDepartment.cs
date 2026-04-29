using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StajSistemi.Migrations
{
    public partial class SupportMultiDepartment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // İLİŞKİLERİ KOPAR
            migrationBuilder.DropForeignKey(
                name: "FK_Internships_Departments_DepartmentId",
                table: "Internships");

            migrationBuilder.DropIndex(
                name: "IX_Internships_DepartmentId",
                table: "Internships");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Internships");

            // KOLONLARI GÜNCELLE
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Internships",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentName",
                table: "Departments",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // YENİ KÖPRÜ TABLOMUZU KUR
            migrationBuilder.CreateTable(
                name: "InternshipDepartments",
                columns: table => new
                {
                    InternshipId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternshipDepartments", x => new { x.InternshipId, x.DepartmentId });
                    table.ForeignKey(
                        name: "FK_InternshipDepartments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InternshipDepartments_Internships_InternshipId",
                        column: x => x.InternshipId,
                        principalTable: "Internships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // GÜVENLİK GÜNCELLEMESİ
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8498cc08-4a6e-43a0-8e01-20bdc509e24a", "AQAAAAIAAYagAAAAEIWtKPHSVefARUTW8wnxQykpy46ggc9nWGAolsCI88KLLPdvhf5qVD0gS5z6Y2SB4A==", "74b708a2-1759-4f7d-820f-87d1a56cb1c1" });

            migrationBuilder.CreateIndex(
                name: "IX_InternshipDepartments_DepartmentId",
                table: "InternshipDepartments",
                column: "DepartmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "InternshipDepartments");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Internships",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Internships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentName",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9bcd9db1-0c62-4fa7-b2c5-540e20664e0a", "AQAAAAIAAYagAAAAEHd5ooI7Ghv+a9XR6Q3lBZN/dxv1UvKDIfgFMui7GMMLVU9b/qPc4xiQboEzikzeqg==", "58dc1d64-f300-4467-9ff2-4cde8ffbb4ce" });

            migrationBuilder.CreateIndex(
                name: "IX_Internships_DepartmentId",
                table: "Internships",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Internships_Departments_DepartmentId",
                table: "Internships",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}