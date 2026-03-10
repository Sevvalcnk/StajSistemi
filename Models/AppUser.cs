using Microsoft.AspNetCore.Identity;

namespace StajSistemi.Models // Namespace adının doğruluğundan emin ol
{
    // IdentityUser<int> kullandığın için kullanıcı ID'lerin sayısal (1, 2, 3...) olacak.
    public class AppUser : IdentityUser<int>
    {
        public string FullName { get; set; } // Ad Soyad için

        public string? IPAddress { get; set; } // Güvenlik için loglayacağımız IP (Hafta 3)

        // --- YENİ EKLENEN ALAN ---
        // Sadece öğrenciler için kullanılacak okul numarası alanı
        public string? StudentNo { get; set; }

    }
}