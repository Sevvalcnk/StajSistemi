using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StajSistemi.Models
{
    public class Department
    {
        [Key] // Birincil Anahtar (PK)
        public int Id { get; set; }

        [Required(ErrorMessage = "Bölüm adı alanı boş bırakılamaz.")]
        [Display(Name = "Bölüm Adı")]
        [StringLength(150, ErrorMessage = "Bölüm adı en fazla 150 karakter olabilir.")]
        public string DepartmentName { get; set; }

        // ✅ MEVCUT YAPI: Bir bölümde birden fazla kullanıcı (öğrenci) olabilir.
        // AppUser ile olan One-to-Many (Bire-Çok) ilişkisi harfiyen korundu.
        public virtual ICollection<AppUser>? Students { get; set; }

        [Display(Name = "Eğitim Düzeyi")]
        public string? DegreeLevel { get; set; } // "Önlisans" veya "Lisans"

        // 🚀 YENİ SİBER MÜHÜR (Many-to-Many): 
        // Bu mühür sayesinde, bir ilanı (Internship) birden fazla bölüme asaletle bağlayabiliyoruz.
        // 'virtual' kelimesi Entity Framework'ün bu veriyi daha akıllı (Lazy Loading) çekmesini sağlar.
        public virtual ICollection<InternshipDepartment> InternshipDepartments { get; set; } = new List<InternshipDepartment>();
    }
}