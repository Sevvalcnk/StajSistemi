using Microsoft.AspNetCore.Http;

namespace StajSistemi.Models.ViewModels
{
    public class StudentProfileViewModel
    {
        public string? FullName { get; set; }
        public string? StudentNo { get; set; }
        public string? Email { get; set; }
        public double? GPA { get; set; }
        public string? PersonalSkills { get; set; }
        public string? EducationSummary { get; set; }
        public string? PhoneNumber { get; set; }

        // ✅ Dosya yükleme için kritik alanlar
        public IFormFile? CVFile { get; set; }
        public IFormFile? CertificateFile { get; set; }

        // Mevcut dosyaları görmek için
        public string? ExistingCVPath { get; set; }
        public string? ExistingCertPath { get; set; }
    }
}