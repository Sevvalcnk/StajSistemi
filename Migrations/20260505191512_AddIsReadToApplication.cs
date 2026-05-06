using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddIsReadToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReadByStudent",
                table: "InternshipApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0c222918-b0df-445c-a3f6-693aebb79f51", "AQAAAAIAAYagAAAAEBfYi+mzWtbKdad5lYZfsMZ7CYgGUXwCIK5tO8Bf7Co+EUcVLQQcB+S4dJB4DLzV3A==", "39366918-f729-4c72-96c2-5ad920bc8e9c" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReadByStudent",
                table: "InternshipApplications");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "99969980-9dbe-4731-b983-8528d54b5df8", "AQAAAAIAAYagAAAAELG2lVfeQVSmnTyuASUAhvq7YQ7oeH4cyXMH8c0YYmCW7zyGpsRxehHjOybj4wHSXQ==", "eb9e42f4-0395-4e5a-8783-ebc8155b444d" });
        }
    }
}
