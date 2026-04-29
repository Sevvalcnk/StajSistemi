using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        // 🛡️ SİBER DÜZELTME: [Required] mühürü buradan kaldırıldı.
        // Neden? Çünkü Admin ve Hocaların bölümü olmaz, sistem açılışta hata vermesin diye.
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public string? DepartmentName { get; set; }
        public int? AdvisorId { get; set; }

        public string? UniversityName { get; set; }
        public string? FacultyName { get; set; }
        public string? AcademicYear { get; set; }

        public string? BirthPlace { get; set; }
        public DateTime? BirthDate { get; set; }

        public string? CompanyName { get; set; }
        public string? CompanySector { get; set; }
        public string? CompanyTaxNumber { get; set; }
        public string? DegreeType { get; set; }

        // 🛡️ SİBER DÜZELTME: [Required] buradan da kaldırıldı.
        public string? EducationLevel { get; set; }

        public ICollection<InternshipApplication>? Applications { get; set; }
    }
}