using System.ComponentModel.DataAnnotations;

namespace StajSistemi.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; }

        // Sadece bu kalsın, diğeri fazla olduğu için hata veriyordu
        [Display(Name = "Öğrenci Numarası")]
        public string? StudentNo { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Lütfen bir rol seçiniz.")]
        public string Role { get; set; } // Student, Advisor, Admin
    }
}