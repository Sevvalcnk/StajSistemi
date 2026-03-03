using System.Linq.Expressions;

namespace StajSistemi.Repositories.Abstract
{
    // T : class kuralı, bu yapının sadece veritabanı tablolarıyla (Student, Admin vb.) çalışacağını garanti eder.
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(); // Tüm listeyi getirir 
        Task<T> GetByIdAsync(int id); // Sadece bir tane kaydı ID'ye göre getirir 
        Task AddAsync(T entity); // Yeni kayıt ekler 
        void Update(T entity); // Var olan kaydı günceller 
        void Delete(T entity); // Kaydı siler 
    }
}