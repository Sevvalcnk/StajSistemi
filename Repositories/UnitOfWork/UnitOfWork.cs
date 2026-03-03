using StajSistemi.data; // DbContext için
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;
using StajSistemi.Repositories.Concrete;

namespace StajSistemi.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context; // Veritabanı bağlantısı
        private IGenericRepository<Student> _students;
        private IGenericRepository<Department> _departments;
        private IGenericRepository<Advisor> _advisors;
        private IGenericRepository<Admin> _admins;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // Eğer depo daha önce oluşturulmadıysa oluştur (Lazy Loading)
        public IGenericRepository<Student> Students => _students ??= new GenericRepository<Student>(_context);
        public IGenericRepository<Department> Departments => _departments ??= new GenericRepository<Department>(_context);
        public IGenericRepository<Advisor> Advisors => _advisors ??= new GenericRepository<Advisor>(_context);
        public IGenericRepository<Admin> Admins => _admins ??= new GenericRepository<Admin>(_context);

        public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}