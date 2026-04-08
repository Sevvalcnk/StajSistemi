using StajSistemi.data;
using StajSistemi.Models;
using StajSistemi.Repositories.Abstract;
using StajSistemi.Repositories.Concrete;

namespace StajSistemi.Repositories.Concrete
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        // 🛡️ ÖZEL DEĞİŞKENLER (Private Fields)
        private IGenericRepository<AppUser> _students;
        private IGenericRepository<Department> _departments;
        private IGenericRepository<InternshipApplication> _internshipApplications;
        private IGenericRepository<Internship> _internships;
        private IGenericRepository<City> _cityRepository;

        // ✅ YENİ MÜHÜR: Mesajlaşma çekmecesi değişkeni
        private IGenericRepository<ChatMessage> _chatMessages;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🛡️ GENEL ÖZELLİKLER (Public Properties)
        public IGenericRepository<AppUser> Students => _students ??= new GenericRepository<AppUser>(_context);
        public IGenericRepository<Department> Departments => _departments ??= new GenericRepository<Department>(_context);
        public IGenericRepository<InternshipApplication> InternshipApplications => _internshipApplications ??= new GenericRepository<InternshipApplication>(_context);
        public IGenericRepository<Internship> Internships => _internships ??= new GenericRepository<Internship>(_context);
        public IGenericRepository<City> Cities => _cityRepository ??= new GenericRepository<City>(_context);

        // ✅ YENİ MÜHÜR: Akıllı Sohbet Sistemine Erişim Özelliği
        public IGenericRepository<ChatMessage> ChatMessages => _chatMessages ??= new GenericRepository<ChatMessage>(_context);

        // 🛡️ İŞLEMLER
        public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}