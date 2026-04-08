using StajSistemi.Models;

namespace StajSistemi.Repositories.Abstract
{
    public interface IUnitOfWork : IDisposable
    {
        // 🛡️ Kullanıcı Yönetimi (Hepsi AppUser üzerinden döner)
        IGenericRepository<AppUser> Students { get; }

        // ✅ Diğer Tablolar
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<InternshipApplication> InternshipApplications { get; }
        IGenericRepository<Internship> Internships { get; }

        // 💬 YENİ MÜHÜR: Akıllı Sohbet Sistemi Deposu (Hata veren kısım burasıydı)
        IGenericRepository<ChatMessage> ChatMessages { get; }

        // 🛡️ MÜHÜR: Şehirler çekmecesini alet çantasına tanımlıyoruz
        IGenericRepository<City> Cities { get; }

        Task<int> SaveAsync();
    }
}