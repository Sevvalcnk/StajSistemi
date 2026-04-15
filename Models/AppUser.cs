using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace StajSistemi.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? StudentNo { get; set; }
        public string? IPAddress { get; set; }

        // 🚀 ÖĞRENCİ PANELİ ALANLARI
        public double? GPA { get; set; }
        public string? PersonalSkills { get; set; }
        public string? EducationSummary { get; set; }
        public string? CVPath { get; set; }
        public string? CertificatePath { get; set; }

        // ✅ ŞEHİR VE BÖLÜM İLİŞKİLERİ
        public int? CityId { get; set; }
        public City? City { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        // 🛡️ BÖLÜM ADI (ADMIN PANELİ VE ÇIKTILAR İÇİN)
        public string? DepartmentName { get; set; }

        public int? AdvisorId { get; set; }

        // 🏛️ GENEL ÜNİVERSİTE VE KAPAK SAYFASI MÜHÜRLERİ (DİNAMİK HALE GELDİ)
        public string? UniversityName { get; set; } // ✅ Sabit yazı kaldırıldı, artık Türkiye geneli!
        public string? FacultyName { get; set; }
        public string? AcademicYear { get; set; }

        // 👤 RESMİ KİMLİK KÜNYESİ (PDF ÇIKTISI İÇİN ŞART)
        public string? BirthPlace { get; set; }
        public DateTime? BirthDate { get; set; }

        // 🏢 STAJ YAPILAN KURUM KÜNYESİ
        public string? CompanyName { get; set; }
        public string? CompanySector { get; set; }
        public string? CompanyTaxNumber { get; set; }
        // AppUser.cs veya Student.cs içine diğer özelliklerin yanına ekle:
        public string? DegreeType { get; set; }
        // ✅ İLİŞKİLER
        public ICollection<InternshipApplication>? Applications { get; set; } // Model ismin neyse (Application veya InternshipApplication) ona göre güncelleyebilirsin.
    }
}