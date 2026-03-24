using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StajSistemi.Models
{
    public class Internship
    {
        public int Id { get; set; }

        // ✅ MÜHÜR: Eğer formda başlık alanı kullanmayacaksan bunu nullable (?) yapmalısın.
        // Ya da Controller tarafında bunu otomatik dolduracağız.
        [Required(ErrorMessage = "İlan başlığı zorunludur.")]
        public string Name { get; set; }

        public string? CompanyName { get; set; }
        public string? City { get; set; }
        public string? CompanySector { get; set; }

        [Range(1, 100, ErrorMessage = "Kontenjan 1-100 arasında olmalıdır.")]
        public int Quota { get; set; }

        // ✅ NOT: View'da @item.Department.DepartmentName kullanacağımız için bu alana teknik olarak gerek yok, 
        // ama veritabanında dursun diyorsan kalabilir.
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
        public string? Status { get; set; } = "Aktif";
        public bool IsDeleted { get; set; } = false;

        // ✅ IDENTITY MÜHRÜ: Senin AppUser modelinde Id 'int' ise burası 'int?' kalmalı.
        // Eğer Identity varsayılan (string) kullanıyorsa burayı 'string?' yapmalısın.
        public int? AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }
    }
}