using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasonToApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "InternshipApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ffed6fd2-ed93-4c4b-a9e3-0ec39feace77", "AQAAAAIAAYagAAAAEMGLcIp33Za/5ctr0mxgC3os61LDLzDff15/Y7PduUSI+2zJIJya5lX+oe930mzzzA==", "0bd20180-1476-4ae1-ab36-2329703939da" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "InternshipApplications");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b46cff78-2d6d-4eab-be0e-3c087c6c4606", "AQAAAAIAAYagAAAAELZQQeB7JBB40mHT8wYcygMvYbEmSsewWBIV9+yQuBO0LWXm+TCUkPVd4XflTBAdzw==", "b1cc54b1-673a-44d7-8071-c41c6f7ef723" });
        }
    }
}
