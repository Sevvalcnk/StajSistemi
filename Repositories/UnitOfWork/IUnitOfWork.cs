using StajSistemi.Models;

namespace StajSistemi.Repositories.Abstract
{
    public interface IUnitOfWork : IDisposable
    {
        // 🛡️ Kullanıcı Yönetimi (Hepsi AppUser üzerinden döner)
        // 'Students' ismini koruyabilirsin ama tipini AppUser yapmalısın.
        IGenericRepository<AppUser> Students { get; }

        // ✅ Diğer Tablolar
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<InternshipApplication> InternshipApplications { get; }
        IGenericRepository<Internship> Internships { get; }

        Task<int> SaveAsync();
    }
}