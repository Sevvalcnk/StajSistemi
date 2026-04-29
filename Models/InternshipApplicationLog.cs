using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajSistemi.Models
{
    public partial class InternshipApplicationLog
    {
        public int Id { get; set; }

        // ✅ SİBER NİZAM: Foreign Key ismini standart nizamda mühürledik
        public int InternshipApplicationId { get; set; }

        [ForeignKey("InternshipApplicationId")]
        public virtual InternshipApplication? InternshipApplication { get; set; }

        public ApplicationStatus OldStatus { get; set; } // Eski hali neydi?
        public ApplicationStatus NewStatus { get; set; } // Yeni hali ne oldu?

        public string? ChangedBy { get; set; } // Kim değiştirdi? (Admin/Advisor adı)

        // ✅ KRİTİK DÜZELTME: Controller'da hata veren 'LogDate' mühürü buraya işlendi!
        public DateTime LogDate { get; set; } = DateTime.Now;

        // ✅ KRİTİK DÜZELTME: Görünümde (View) 'Açıklama' olarak basılacak olan 'Comment' mühürü!
        public string? Comment { get; set; }
    }
}