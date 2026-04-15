using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StajSistemi.Models;
using System;

namespace StajSistemi.data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Internship> Internships { get; set; }
        public DbSet<InternshipApplication> InternshipApplications { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<DailyReport> DailyReports { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        public DbSet<FileLog> FileLogs { get; set; }
        public DbSet<InternshipApplicationLog> InternshipApplicationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ 1. HASSAS AYARLAR
            modelBuilder.Entity<Internship>()
                .Property(i => i.MinGPA)
                .HasPrecision(3, 2);

            // 🛡️ 2. DAILYREPORT İLİŞKİLERİ
            modelBuilder.Entity<DailyReport>(entity =>
            {
                entity.ToTable("DailyReports");

                entity.HasOne(dr => dr.InternshipApplication)
                    .WithMany(ia => ia.DailyReports)
                    .HasForeignKey(dr => dr.InternshipApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(dr => dr.AppUser)
                    .WithMany()
                    .HasForeignKey(dr => dr.AppUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- 3. ROLLERİ MÜHÜRLE ---
            modelBuilder.Entity<AppRole>().HasData(
                new AppRole { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new AppRole { Id = 2, Name = "Student", NormalizedName = "STUDENT" },
                new AppRole { Id = 3, Name = "Advisor", NormalizedName = "ADVISOR" }
            );

            // --- 4. VARSAYILAN ADMİNİ MÜHÜRLE ---
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
                SecurityStamp = Guid.NewGuid().ToString(),
                UniversityName = "Sinop Üniversitesi",
                FacultyName = "Ayancık Meslek Yüksekokulu",
                DepartmentName = "Yönetim Paneli",
                AcademicYear = "2025-2026",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });

            modelBuilder.Entity<IdentityUserRole<int>>().HasData(new IdentityUserRole<int> { RoleId = 1, UserId = 1 });

            // --- ✅ 5. BÖLÜMLERİ GERİ UYANDIRDIK ---
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, DepartmentName = "İnternet ve Ağ Teknolojileri" },
                new Department { Id = 2, DepartmentName = "Çocuk Gelişimi Programı" },
                new Department { Id = 3, DepartmentName = "Dış Ticaret Programı" },
                new Department { Id = 4, DepartmentName = "Kontrol ve Otomasyon Teknolojisi Programı" },
                new Department { Id = 5, DepartmentName = "Ormancılık ve Orman Teknolojisi Programı" },
                new Department { Id = 6, DepartmentName = "İç Mekan Tasarım Programı" },
                new Department { Id = 7, DepartmentName = "Lojistik Programı" },
                new Department { Id = 8, DepartmentName = "Yapay Zeka" },
                new Department { Id = 9, DepartmentName = "Dijital Oyun Tasarımı ve Geliştirme" }
            );

            // --- 🌍 8. GÜNCEL ŞEHİR LİSTESİ (15 ŞEHİR) ---
            modelBuilder.Entity<City>().HasData(
                new City { Id = 1, Name = "Sinop" },
                new City { Id = 2, Name = "Samsun" },
                new City { Id = 3, Name = "İstanbul" },
                new City { Id = 4, Name = "Ankara" },
                new City { Id = 5, Name = "İzmir" },
                new City { Id = 6, Name = "Kocaeli" },
                new City { Id = 7, Name = "Bursa" },
                new City { Id = 8, Name = "Sakarya" },
                new City { Id = 9, Name = "Kastamonu" },
                new City { Id = 10, Name = "Eskişehir" }, // Çorum yerine
                new City { Id = 11, Name = "Ordu" },
                new City { Id = 12, Name = "Aydın" },     // Giresun yerine
                new City { Id = 13, Name = "Trabzon" },
                new City { Id = 14, Name = "Muğla" },     // Rize yerine
                new City { Id = 15, Name = "Antalya" }
            );

            // --- 💬 6. SOHBET İLİŞKİLERİ ---
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            // --- 🔥 7. TRIGGER ---
            modelBuilder.Entity<InternshipApplication>()
                .ToTable(tb => tb.HasTrigger("trg_ApplicationLog"));
        }
    }
}