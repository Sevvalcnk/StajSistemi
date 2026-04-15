using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajSistemi.Models
{
    public class Internship
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "İlan başlığı zorunludur.")]
        public string Name { get; set; }

        public string? CompanyName { get; set; }

        // ✅ MÜHÜR: Çakışma olmaması için ismini 'CityName' yaptık. 
        public string? CityName { get; set; }

        public string? CompanySector { get; set; }

        [Range(1, 100, ErrorMessage = "Kontenjan 1-100 arasında olmalıdır.")]
        public int Quota { get; set; }

        public string? DepartmentName { get; set; }

        [Required(ErrorMessage = "Lütfen bir bölüm seçiniz.")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi seçilmelidir.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi seçilmelidir.")]
        public DateTime EndDate { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // 🔥 KRİTİK DOKUNUŞ: Artık "string" değil, akıllı "ApplicationStatus" mühürü!
        // Varsayılan olarak "Active" (yani 6) değerini alıyor.
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Active;

        public bool IsDeleted { get; set; } = false;

        public int? AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }

        // 🚀 HAFTA 6: AKILLI FİLTRELEME ALANLARI
        public decimal MinGPA { get; set; } // Gereken Min. Ortalama

        public int? CityId { get; set; }    // Şehir Kimliği

        [ForeignKey("CityId")]
        public virtual City? City { get; set; }  // Şehir Tablosuyla Bağlantı
                                                 // ✅ MÜHÜR: Başarı skoru alanı modele eklendi
       
    }
}