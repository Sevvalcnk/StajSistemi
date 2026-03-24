using Microsoft.AspNetCore.Identity;

namespace StajSistemi.Models
{
    // IdentityUser<int> ile ID'lerin sayı olacağını mühürledik.
    public class AppUser : IdentityUser<int>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // ✅ FullName alanı: Ad ve Soyadı birleştirmek için kullanılabilir.
        public string? FullName { get; set; }

        public string? StudentNo { get; set; }
        public string? IPAddress { get; set; }

        // 🚀 ÖĞRENCİ PANELİ HATALARINI BİTİREN YENİ ALANLAR:
        // Sistem "GPA nerede? CVPath nerede?" diye ağlıyordu, işte buradalar:

        public double? GPA { get; set; }             // Not Ortalaması
        public string? PersonalSkills { get; set; }   // Kişisel Yetenekler
        public string? EducationSummary { get; set; } // Eğitim Özeti
        public string? CVPath { get; set; }           // CV Dosyasının Yolu (/uploads/cvs/...)
        public string? CertificatePath { get; set; }  // Sertifika Dosyasının Yolu

        // ✅ Soft Delete Kontrolü
        public bool IsDeleted { get; set; } = false;

        // Bölüm İlişkisi
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}