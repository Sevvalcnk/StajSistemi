using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class FinalTimelineDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4be7357a-7033-41fa-80e5-0afb01ef39ac", "AQAAAAIAAYagAAAAENw5sOXTCrfqmPEGpb+pmLI9TJFqWYdbnXaXKsO8M8OeXCP6t4RGPaizTjPT6oy5hA==", "316b32b9-5f43-447a-9a30-71473bea3b24" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "56881dbd-6eb5-48bb-aeff-a8fa2c65e915", "AQAAAAIAAYagAAAAELTrTe/aVfW8+J7b07Pu9RFKVtdWMQXlfg2gR69g3Gp3z6IteVPbUTEQLq36h6B/6A==", "9ca40e8d-c31d-4353-a916-0e56526a7517" });
        }
    }
}
