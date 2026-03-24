using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    public partial class FixIsDeletedSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ BURAYI BOŞALTTIK
            // Veritabanında bu sütunlar zaten olduğu için 
            // kodun tekrar eklemeye çalışıp hata vermesini engelledik.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ BURAYI DA BOŞALTTIK
        }
    }
}