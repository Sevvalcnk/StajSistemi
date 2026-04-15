using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajSistemi.Models
{
    public class InternshipApplicationLog
    {
        public int Id { get; set; }

        public int ApplicationId { get; set; } // Hangi başvuru değişti?
        [ForeignKey("ApplicationId")]
        public virtual InternshipApplication Application { get; set; }

        public ApplicationStatus OldStatus { get; set; } // Eski hali neydi?
        public ApplicationStatus NewStatus { get; set; } // Yeni hali ne oldu?

        public string ChangedBy { get; set; } // Kim değiştirdi? (Admin/Advisor adı)
        public DateTime ChangeDate { get; set; } = DateTime.Now; // Ne zaman?

        public string? Note { get; set; } // Varsa küçük bir açıklama
    }
}