using StajSistemi.Models;
using System.Linq.Expressions;

namespace StajSistemi.Repositories.Abstract
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);

        // ❌ BURADAKİ Internships SATIRINI SİLDİK! 
        // Çünkü bu dosya geneldir, özel bir tablo ismi barındırmaz.

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

        Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includeProperties);
    }
}