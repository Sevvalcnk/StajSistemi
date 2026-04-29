using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class FinalNizamFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipApplicationLogs_InternshipApplications_ApplicationId",
                table: "InternshipApplicationLogs");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "InternshipApplicationLogs",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "ChangeDate",
                table: "InternshipApplicationLogs",
                newName: "LogDate");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "InternshipApplicationLogs",
                newName: "InternshipApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipApplicationLogs_ApplicationId",
                table: "InternshipApplicationLogs",
                newName: "IX_InternshipApplicationLogs_InternshipApplicationId");

            migrationBuilder.AlterColumn<string>(
                name: "ChangedBy",
                table: "InternshipApplicationLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2e48f89d-4b93-4b79-a0ff-a50d9ec7d20e", "AQAAAAIAAYagAAAAEMycICVgW2pRKsUywj4hX4kPTJCjtnePZn8KSkBihZYsgQGKF30Va07H/f5eCzlxBA==", "070655bf-45ab-472b-a1f1-95b668d6e5fe" });

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipApplicationLogs_InternshipApplications_InternshipApplicationId",
                table: "InternshipApplicationLogs",
                column: "InternshipApplicationId",
                principalTable: "InternshipApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipApplicationLogs_InternshipApplications_InternshipApplicationId",
                table: "InternshipApplicationLogs");

            migrationBuilder.RenameColumn(
                name: "LogDate",
                table: "InternshipApplicationLogs",
                newName: "ChangeDate");

            migrationBuilder.RenameColumn(
                name: "InternshipApplicationId",
                table: "InternshipApplicationLogs",
                newName: "ApplicationId");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "InternshipApplicationLogs",
                newName: "Note");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipApplicationLogs_InternshipApplicationId",
                table: "InternshipApplicationLogs",
                newName: "IX_InternshipApplicationLogs_ApplicationId");

            migrationBuilder.AlterColumn<string>(
                name: "ChangedBy",
                table: "InternshipApplicationLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8498cc08-4a6e-43a0-8e01-20bdc509e24a", "AQAAAAIAAYagAAAAEIWtKPHSVefARUTW8wnxQykpy46ggc9nWGAolsCI88KLLPdvhf5qVD0gS5z6Y2SB4A==", "74b708a2-1759-4f7d-820f-87d1a56cb1c1" });

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipApplicationLogs_InternshipApplications_ApplicationId",
                table: "InternshipApplicationLogs",
                column: "ApplicationId",
                principalTable: "InternshipApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
