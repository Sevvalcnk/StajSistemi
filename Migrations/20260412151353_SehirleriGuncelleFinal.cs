using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class SehirleriGuncelleFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b13ec2ad-43f7-43c2-8ef3-16b2be7cfcea", "AQAAAAIAAYagAAAAEG/rJaxugqT10M9NTrC5RLIkSy2YIZAUvtItYhjKt6lPir0OR03nfKxka1csg6idzw==", "1e321f79-69f7-4a91-9b26-69c6ae98e4f0" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Sinop" },
                    { 2, "Samsun" },
                    { 3, "İstanbul" },
                    { 4, "Ankara" },
                    { 5, "İzmir" },
                    { 6, "Kocaeli" },
                    { 7, "Bursa" },
                    { 8, "Sakarya" },
                    { 9, "Kastamonu" },
                    { 10, "Eskişehir" },
                    { 11, "Ordu" },
                    { 12, "Aydın" },
                    { 13, "Trabzon" },
                    { 14, "Muğla" },
                    { 15, "Antalya" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b9d8f0c4-4d5a-4d27-9835-0aafa8c9e8c3", "AQAAAAIAAYagAAAAEOLZj8+POtrsfliyMyGmKxhRxa4OLS8fU+M7druYavXCxTi7u0Rud5ABsqr1WJL4Iw==", "16229fac-99f0-4056-8b3d-5b78bb2f3fb5" });
        }
    }
}
