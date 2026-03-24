using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StajSistemi.Models
{
    public class Department
    {
        [Key] // Birincil Anahtar (PK)
        public int Id { get; set; }

        [Required] // Boş geçilemez
        [Display(Name = "Bölüm Adı")]
        public string DepartmentName { get; set; }

        // ✅ DÜZELTME: Artık bölüme bağlı olanlar eski 'Student' değil, 'AppUser' modelidir.
        // İlişki: Bir bölümde birden fazla kullanıcı (öğrenci) olabilir.
        public ICollection<AppUser>? Students { get; set; }
    }
}