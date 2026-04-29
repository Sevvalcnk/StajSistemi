using System.ComponentModel.DataAnnotations;

namespace StajSistemi.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-posta adresi boş bırakılamaz.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Lütfen geçerli bir e-posta adresi giriniz (Örn: isim@alanadi.com)")]
        public string Email { get; set; }

        [Display(Name = "Üniversite Adı")]
        public string? UniversityName { get; set; }

        [Display(Name = "Fakülte veya Meslek Yüksekokulu")]
        public string? FacultyName { get; set; }

        // 🛡️ ÖĞRENCİ NO: Kayıt sayfasında öğrenci seçilince göründüğü için kalsın 
        // ama hata almamak için Required uyarısını sadeleştirdik.
        [Display(Name = "Öğrenci Numarası")]
        public string? StudentNo { get; set; }

        // ❌ BURADAKİ [Required] MÜHÜRLERİNİ KALDIRDIK
        [Display(Name = "Eğitim Düzeyi")]
        public string? EducationLevel { get; set; }

        [Display(Name = "Okuduğunuz Bölüm")]
        public int? DepartmentId { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Lütfen bir rol seçiniz.")]
        public string Role { get; set; }
    }
}