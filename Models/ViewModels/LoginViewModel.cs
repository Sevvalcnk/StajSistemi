using System.ComponentModel.DataAnnotations;

namespace StajSistemi.Models.ViewModels
{
    public class LoginViewModel
    {
        // 1. [EmailAddress] etiketini sildik ki numara ile girişe izin versin
        [Required(ErrorMessage = "Lütfen kullanıcı adı, numara veya e-posta giriniz.")]
        public string Email { get; set; } // Unuttuğun satırı buraya ekledim!

        [Required(ErrorMessage = "Lütfen şifrenizi giriniz.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; } // Hafta 3: Cookie yönetimi için
    }
}