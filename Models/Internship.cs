using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajSistemi.Models
{
    public class Internship
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "İlan başlığı zorunludur.")]
        [StringLength(200)]
        public string Name { get; set; }

        public string? CompanyName { get; set; }

        public string? CityName { get; set; }

        public string? CompanySector { get; set; }

        [Range(1, 100, ErrorMessage = "Kontenjan 1-100 arasında olmalıdır.")]
        public int Quota { get; set; }

        // ✅ MÜHÜR: Hızlı listeleme ve raporlama için bu alan kalsın.
        public string? DepartmentName { get; set; }

        // 🛡️ SİBER DÜZELTME: 'DepartmentId' kaldırıldı. 
        // 🚀 YENİ MÜHÜR: Çoklu Bölüm Desteği (Many-to-Many Bridge)
        // Artık bu ilan üzerinden köprü tabloya (InternshipDepartments) asaletle ulaşıyoruz.
        public virtual ICollection<InternshipDepartment> InternshipDepartments { get; set; } = new List<InternshipDepartment>();

        [Required(ErrorMessage = "Başlangıç tarihi seçilmelidir.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi seçilmelidir.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // ✅ Not: ApplicationStatus Enum yapısının projenizde mevcut olduğunu biliyoruz.
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Active;

        public bool IsDeleted { get; set; } = false;

        public int? AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }

        // 🚀 KRİTİK EKSİK: Veritabanında hassasiyet hatası almamak için 
        // MinGPA alanına mühür bastık (DbContext'teki precision ile uyumlu hale getirdik).
        [Column(TypeName = "decimal(3, 2)")]
        public decimal MinGPA { get; set; }

        public int? CityId { get; set; }

        [ForeignKey("CityId")]
        public virtual City? City { get; set; }
    }
}