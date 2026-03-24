using System;
using System.ComponentModel.DataAnnotations;

namespace StajSistemi.Models
{
    public class Application
    {
        public int Id { get; set; }

        // 🛡️ HAFTA 4: Başvuru Tarihi ve Durumu
        public DateTime ApplicationDate { get; set; } = DateTime.Now;
        public string? Status { get; set; } = "Beklemede"; // Onaylandı, Beklemede, Reddedildi

        // ✅ KRİTİK DÜZELTME: Artık 'Student' yok, 'AppUser' var.
        // Bu başvuran öğrencinin Identity ID'sini tutar.
        public int AppUserId { get; set; }
        public virtual AppUser? AppUser { get; set; }

        // ✅ İLİŞKİ MÜHÜRÜ: Bu başvuru hangi staj ilanı için?
        public int InternshipId { get; set; }
        public virtual Internship? Internship { get; set; }
    }
}