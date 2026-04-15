namespace StajSistemi.Models
{
    public enum ApplicationStatus
    {
        // --- 📋 BAŞVURU DURUMLARI ---
        Pending = 0,        // Beklemede (İlk başvuru anı)
        InReview = 1,       // Danışman inceliyor
        Approved = 2,       // Onaylandı!
        Rejected = 3,       // Reddedildi (Hocanın dürüst veri ilkesi)
        Completed = 4,      // Staj başarıyla bitti
        MissingDocument = 5, // Belge eksik uyarısı

        // --- 📢 İLAN VE ARŞİV DURUMLARI (Kritik Eklemeler) ---
        // Bu iki etiket sayesinde InternshipController ve StudentPanel hata vermeyi bırakacak.
        Active = 6,         // İlan yayında ve başvurulara açık
        Deleted = 7         // İlan yayından kaldırıldı (Arşivde)
    }
}