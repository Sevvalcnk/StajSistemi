using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvisorIdToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdvisorId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AdvisorId", "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { null, "817e7654-619f-4221-869c-f55a5bbe7b28", "AQAAAAIAAYagAAAAEOrU88t+nhbdHfvwgVc6/nk+MNQ2gWY1qM7EUbRja3nKYXSzWw6dolc4Z5NfUbbxKQ==", "179348d2-99da-40e5-8afb-dfff36c5510b" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdvisorId",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "95a5416a-e687-4dc7-80ac-0bd9c32c6b51", "AQAAAAIAAYagAAAAEASU6wusHCniinSqI8sUnXNWoUNoDdac8w2KLVddS9NquGz8tjvoLDrk16ThYYr1ig==", "1d73a58f-0eb9-4902-a890-2caacfd01567" });
        }
    }
}
