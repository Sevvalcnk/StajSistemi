using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajSistemi.Models
{
    public class InternshipApplication
    {
        public int Id { get; set; }

        // ✅ IDENTITY MÜHÜRÜ
        public int AppUserId { get; set; }
        public virtual AppUser? AppUser { get; set; }

        public int InternshipId { get; set; }
        public virtual Internship? Internship { get; set; }

        // 📂 HAFTA 5: Dosya Yönetimi
        public string? CVPath { get; set; }
        public string? CertificatePath { get; set; }

        // 🛡️ HAFTA 4 & 7: Kontrol ve Workflow
        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        // 📅 KRİTİK EKLEME: Stajın Gerçek Zaman Çizelgesi
        // Bu iki satır Documents sayfasındaki o kırmızı hataları silecek! 🚀
        public DateTime? StartDate { get; set; } // Staj Başlangıç Tarihi
        public DateTime? EndDate { get; set; }   // Staj Bitiş Tarihi

        // ESKİ string Status SİLİNDİ! Artık akıllı Enum mühürü var:
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public bool IsDeleted { get; set; } = false;
        public string? StudentIP { get; set; }
        public double SuccessScore { get; set; }

        // ✅ HAFTA 7 MÜHÜRÜ: Dijital Staj Defteri Bağlantısı
        public virtual ICollection<DailyReport> DailyReports { get; set; } = new List<DailyReport>();
    }
}