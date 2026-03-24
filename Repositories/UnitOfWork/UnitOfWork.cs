using StajSistemi.data;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;
using StajSistemi.Repositories.Concrete;

namespace StajSistemi.Repositories.Concrete
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        // ✅ GÜNCELLEME: Sadece var olan modeller için depoları tanımlıyoruz
        private IGenericRepository<AppUser> _students;
        private IGenericRepository<Department> _departments;
        private IGenericRepository<InternshipApplication> _internshipApplications;
        private IGenericRepository<Internship> _internships;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ MÜHÜR: Tüm kullanıcı işlemleri (Öğrenci, Danışman, Admin) artık buradan, AppUser üzerinden yürüyecek.
        public IGenericRepository<AppUser> Students => _students ??= new GenericRepository<AppUser>(_context);

        public IGenericRepository<Department> Departments => _departments ??= new GenericRepository<Department>(_context);
        public IGenericRepository<InternshipApplication> InternshipApplications => _internshipApplications ??= new GenericRepository<InternshipApplication>(_context);
        public IGenericRepository<Internship> Internships => _internships ??= new GenericRepository<Internship>(_context);

        public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}