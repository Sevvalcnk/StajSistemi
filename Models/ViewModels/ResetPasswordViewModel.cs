using System.ComponentModel.DataAnnotations;

namespace StajSistemi.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } // Mailden gelen gizli anahtar

        [Required]
        public string Email { get; set; } // Şifresi sıfırlanan kişi

        [Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}