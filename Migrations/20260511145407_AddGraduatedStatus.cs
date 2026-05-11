using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddGraduatedStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b46cff78-2d6d-4eab-be0e-3c087c6c4606", "AQAAAAIAAYagAAAAELZQQeB7JBB40mHT8wYcygMvYbEmSsewWBIV9+yQuBO0LWXm+TCUkPVd4XflTBAdzw==", "b1cc54b1-673a-44d7-8071-c41c6f7ef723" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b8c319de-a076-4a16-b265-c9eed64882ec", "AQAAAAIAAYagAAAAENbsBHC2QKGVmrx+xY1MfiTHm9ysnAZJLBr4pDiMyPa1eV2DcpYwhd9drWOSzZZ9HA==", "d8275fb3-e56a-4ac4-92ed-ef13cb558ffb" });
        }
    }
}
