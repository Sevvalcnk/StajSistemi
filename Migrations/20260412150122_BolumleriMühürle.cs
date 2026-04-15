using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class BolumleriMühürle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b9d8f0c4-4d5a-4d27-9835-0aafa8c9e8c3", "AQAAAAIAAYagAAAAEOLZj8+POtrsfliyMyGmKxhRxa4OLS8fU+M7druYavXCxTi7u0Rud5ABsqr1WJL4Iw==", "16229fac-99f0-4056-8b3d-5b78bb2f3fb5" });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "DepartmentName" },
                values: new object[,]
                {
                    { 1, "İnternet ve Ağ Teknolojileri" },
                    { 2, "Çocuk Gelişimi Programı" },
                    { 3, "Dış Ticaret Programı" },
                    { 4, "Kontrol ve Otomasyon Teknolojisi Programı" },
                    { 5, "Ormancılık ve Orman Teknolojisi Programı" },
                    { 6, "İç Mekan Tasarım Programı" },
                    { 7, "Lojistik Programı" },
                    { 8, "Yapay Zeka" },
                    { 9, "Dijital Oyun Tasarımı ve Geliştirme" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2);

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

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ab641e33-11dc-4bfc-9839-e7ec985be37b", "AQAAAAIAAYagAAAAEBgTCVOuinZcKJQx6JH5g9c/qGM3Og3kDZSaPDvYhMFEmioBBy9xEuDqkmWwWXQ9Zg==", "6fd30a90-5f95-4eba-9dea-1c4dc7e32f52" });
        }
    }
}
