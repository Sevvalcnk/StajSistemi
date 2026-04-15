using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class StajTarihleriMuhurlendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "InternshipApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "InternshipApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "474d0de4-5769-4fff-87bb-935f440a4251", "AQAAAAIAAYagAAAAEA9ouE+kgynRF7qas3jlh5oIF8iFQcfuY9EdPo4JLA8DQLU8v74gPqYud8iIz+LW6Q==", "2c0e1df2-66a4-4f38-82db-8c6f8493e3de" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "InternshipApplications");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "InternshipApplications");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "61378acf-0813-4748-a024-5a7136d9d8d0", "AQAAAAIAAYagAAAAEIHo8ryyAap6cWZ1sP3/Z7fOjZLtcNrUuCUQ+/PqrbtC0CjxZj1vrE3pFNCYQSe5mQ==", "81f31787-2b79-46ca-836c-1bf48612d8fd" });
        }
    }
}
