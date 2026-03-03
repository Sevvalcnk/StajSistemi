using StajSistemi.Repositories.Abstract;
using StajSistemi.Models; // Modellerine erişmek için

namespace StajSistemi.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        // Tüm tabloların depolarına tek merkezden ulaşacağız
        IGenericRepository<Student> Students { get; }
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<Advisor> Advisors { get; }
        IGenericRepository<Admin> Admins { get; }

        // Her şeyi tek seferde veritabanına kaydetme emri
        Task<int> SaveAsync();
    }
}