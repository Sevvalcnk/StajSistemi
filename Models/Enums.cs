namespace StajSistemi.Models
{
    public enum ApplicationStatus
    {
        // --- 📋 BAŞVURU VE SÜREÇ DURUMLARI ---
        Pending = 0,        // Beklemede (İlk başvuru anı)
        InReview = 1,       // Danışman inceliyor
        Approved = 2,       // Danışman Onayladı (Raporlar tescillendi)
        Rejected = 3,       // Reddedildi (Hocanın dürüst veri ilkesi)
        Completed = 4,      // Staj başarıyla bitti (İş günü doldu)
        MissingDocument = 5, // Belge eksik uyarısı

        // --- 📢 İLAN VE ARŞİV DURUMLARI ---
        Active = 6,         // İlan yayında ve başvurulara açık
        Deleted = 7,        // İlan yayından kaldırıldı (Arşivde)

        // --- 🎓 MEZUNİYET VE FİNAL TESCİL (YENİ) ---
        // Admin bu statüyü verdiğinde öğrenciye PDF maili tetiklenecek!
        Graduated = 8       // Siber Mezun (Sistemden resmi çıkış yapıldı)
    }
}