using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajSistemi.Models
{
    public class InternshipApplication
    {
        public int Id { get; set; }

        // ✅ IDENTITY MÜHÜRÜ: 'int' kullanarak AppUser (IdentityUser<int>) ile bağladık.
        public int AppUserId { get; set; }
        public virtual AppUser? AppUser { get; set; }

        public int InternshipId { get; set; }
        public virtual Internship? Internship { get; set; }

        // 📂 HAFTA 5: Dosya Yönetimi
        public string? CVPath { get; set; }
        public string? CertificatePath { get; set; }

        // 🛡️ HAFTA 4: Kontrol ve Workflow
        public DateTime ApplicationDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Beklemede"; // Beklemede, Onaylandı, Reddedildi
        public bool IsDeleted { get; set; } = false;
        public string? StudentIP { get; set; }
    }
}