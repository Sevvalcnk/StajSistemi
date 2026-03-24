using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class SonMühürVeTemizlik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Internships_Students_StudentId",
                table: "Internships");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Internships",
                newName: "AppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Internships_StudentId",
                table: "Internships",
                newName: "IX_Internships_AppUserId");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f2b4d32b-be6f-436c-9fe5-719d110935f8", "AQAAAAIAAYagAAAAEDH6dUQ9rCR4sDW9tyDEfdw5zoZjGEJmwluFWNrGu9k0rm/HI9rVyXc8ts+yxMjkLw==", "48a149db-848e-4d73-82a6-87550c1780ec" });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "DepartmentName" },
                values: new object[,]
                {
                    { 3, "Bilgisayar Programcılığı" },
                    { 4, "Muhasebe ve Vergi Uygulamaları" },
                    { 5, "Mekatronik" },
                    { 6, "Grafik Tasarımı" },
                    { 7, "Lojistik" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Internships_AspNetUsers_AppUserId",
                table: "Internships",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Internships_AspNetUsers_AppUserId",
                table: "Internships");

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "Internships",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Internships_AppUserId",
                table: "Internships",
                newName: "IX_Internships_StudentId");

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    CVPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EducationSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GPA = table.Column<double>(type: "float", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalSkills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "231f8561-eabd-42ca-8a92-7852fa74bc32", "AQAAAAIAAYagAAAAEI9Rx7wNJcqk/aTkKX4XdMGQKpdN4X2FH75Xd9IsfkO6v7L1+G7oyIu8vbpst5Uelg==", "" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_DepartmentId",
                table: "Students",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Internships_Students_StudentId",
                table: "Internships",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }
    }
}
