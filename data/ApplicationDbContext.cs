using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using StajSistemi.Models;

namespace StajSistemi.data
{
    // IdentityUser'ı AppUser, Role'ü AppRole ve ID tipini int olarak mühürledik.
    public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // --- 📋 ANA TABLOLARIMIZ ---
        public DbSet<Department> Departments { get; set; }
        public DbSet<Internship> Internships { get; set; }
        public DbSet<InternshipApplication> InternshipApplications { get; set; }
        public DbSet<City> Cities { get; set; }

        // --- 💬 SOHBET SİSTEMİ TABLOSU (Hafta 9-10) ---
        public DbSet<ChatMessage> ChatMessages { get; set; }

        // --- 🛡️ GÜVENLİK VE LOG TABLOLARI (Hafta 3-5) ---
        public DbSet<LoginLog> LoginLogs { get; set; } // Giriş IP kayıtları
        public DbSet<FileLog> FileLogs { get; set; }   // Dosya yükleme kayıtları

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. ROLLERİ MÜHÜRLE ---
            modelBuilder.Entity<AppRole>().HasData(
                new AppRole { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new AppRole { Id = 2, Name = "Student", NormalizedName = "STUDENT" },
                new AppRole { Id = 3, Name = "Advisor", NormalizedName = "ADVISOR" }
            );

            // --- 2. VARSAYILAN ADMİNİ MÜHÜRLE ---
            var hasher = new PasswordHasher<AppUser>();
            modelBuilder.Entity<AppUser>().HasData(new AppUser
            {
                Id = 1,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@stajsistemi.com",
                NormalizedEmail = "ADMIN@STAJSISTEMI.COM",
                EmailConfirmed = true,
                FirstName = "Süper",
                LastName = "Admin",
                FullName = "Süper Admin",
                PasswordHash = hasher.HashPassword(null, "Admin123!"),
                SecurityStamp = Guid.NewGuid().ToString()
            });

            // Admin kullanıcısını Admin rolüne bağla
            modelBuilder.Entity<IdentityUserRole<int>>().HasData(new IdentityUserRole<int> { RoleId = 1, UserId = 1 });

            // --- 3. BÖLÜMLERİ MÜHÜRLE ---
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, DepartmentName = "İnternet ve Ağ Teknolojileri" },
                new Department { Id = 2, DepartmentName = "Çocuk Gelişimi Programı" },
                new Department { Id = 3, DepartmentName = "Bilgisayar Programcılığı" },
                new Department { Id = 4, DepartmentName = "Muhasebe ve Vergi Uygulamaları" },
                new Department { Id = 5, DepartmentName = "Mekatronik" },
                new Department { Id = 6, DepartmentName = "Grafik Tasarımı" },
                new Department { Id = 7, DepartmentName = "Lojistik" }
            );

            // --- 💬 4. SOHBET İLİŞKİLERİ (Multiple Cascade Paths Çözümü) ---
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- 🔥 5. TRIGGER KORUMASI (Hafta 1) ---
            // SQL tarafında trigger olsa bile EF Core'un hata vermemesi için bu mühür şart!
            modelBuilder.Entity<InternshipApplication>()
                .ToTable(tb => tb.HasTrigger("trg_ApplicationLog"));
        }
    }
}