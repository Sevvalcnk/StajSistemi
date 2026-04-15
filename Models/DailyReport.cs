using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajSistemi.Models
{
    // 🛡️ KRİTİK MÜHÜR: SQL'deki tablo adını buraya zorla öğretiyoruz. 
    // Eğer SQL'de tablonun adı "DailyReports" ise bu hata gidecek.
    [Table("DailyReports")]
    public class DailyReport
    {
        [Key]
        public int Id { get; set; }

        // --- 👤 ÖĞRENCİ BAĞLANTISI ---
        public int AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }

        // --- 📋 STAJ BAŞVURU BAĞLANTISI ---
        public int InternshipApplicationId { get; set; }

        [ForeignKey("InternshipApplicationId")]
        public virtual InternshipApplication? InternshipApplication { get; set; }

        // --- 📅 RAPOR DETAYLARI ---
        [Display(Name = "Staj Günü")]
        [Range(1, 30, ErrorMessage = "Staj raporu 1 ile 30 gün arasında olmalıdır kardaşım! 🛡️")]
        public int DayNumber { get; set; }

        [Display(Name = "Günlük Rapor İçeriği")]
        [Required(ErrorMessage = "Lütfen bugün yaptığınız çalışmaları anlatın kardaşım! 🛡️")]
        public string Content { get; set; }

        // ✅ Görsel kanıt (Resim yolu - İnternet kablosu resmi vb. buraya gelecek)
        public string? ImagePath { get; set; }

        // ✅ Kayıt Tarihi
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // ✅ Danışman Onayı
        public bool IsApproved { get; set; } = false;
    }
}