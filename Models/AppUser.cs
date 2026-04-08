using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; // ✅ ICollection ve List işlemleri için şart

namespace StajSistemi.Models
{
    // IdentityUser<int> ile ID'lerin sayı olacağını mühürledik.
    public class AppUser : IdentityUser<int>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // ✅ DÜZELTME MÜHRÜ: 
        // Bunu { get; set; } yaptık ki AccountController veya SeedData 
        // buraya isim yazmaya çalıştığında proje hata vermesin, "şak" diye çalışsın.
        public string? FullName { get; set; }

        public string? StudentNo { get; set; }
        public string? IPAddress { get; set; }

        // 🚀 ÖĞRENCİ PANELİ ALANLARI:
        public double? GPA { get; set; }               // Not Ortalaması
        public string? PersonalSkills { get; set; }   // Kişisel Yetenekler
        public string? EducationSummary { get; set; } // Eğitim Özeti
        public string? CVPath { get; set; }           // CV Dosyasının Yolu
        public string? CertificatePath { get; set; }  // Sertifika Dosyasının Yolu

        // ✅ AKILLI FİLTRELEME İÇİN EKLENEN ŞEHİR ALANLARI (HAFTA 6):
        public int? CityId { get; set; }              // Şehir Kimliği
        public City? City { get; set; }               // Şehir Tablosuyla Mühürleme

        // ✅ Soft Delete Kontrolü
        public bool IsDeleted { get; set; } = false;

        // Bölüm İlişkisi
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        // ✅ KRİTİK EKLEME: DANIŞMAN MÜHÜRÜ
        public int? AdvisorId { get; set; }

        // ✅ Kullanıcının yaptığı başvuruları da burada görebilmeliyiz (İlişki için)
        public ICollection<Application>? Applications { get; set; }
    }
}