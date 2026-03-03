using Microsoft.EntityFrameworkCore;
using StajSistemi.Models;

namespace StajSistemi.data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // --- Hafta 1: Ana Tablolar ---
        public DbSet<Student> Students { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Advisor> Advisors { get; set; }
        public DbSet<Admin> Admins { get; set; }

        // --- Hafta 1: Yeni Eklenen Tablolar ---
        public DbSet<Internship> Internships { get; set; }     // Staj Bilgileri
        public DbSet<Application> Applications { get; set; }   // Başvuru Kayıtları
        public DbSet<LoginLog> LoginLogs { get; set; }         // Sisteme Giriş Logları

        // --- ✘ INDEX VE CONSTRAINT TANIMLARI (BURADA TAMAMLANIYOR) ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Student Tablosu Kısıtlamaları
            modelBuilder.Entity<Student>(entity =>
            {
                // Öğrenci Numarası benzersiz (Unique Index) olmalı
                entity.HasIndex(s => s.StudentNo).IsUnique();

                // Email alanı zorunlu ve maksimum 100 karakter
                entity.Property(s => s.Email).IsRequired().HasMaxLength(100);

                // Ad ve Soyad zorunlu olsun
                entity.Property(s => s.Name).IsRequired().HasMaxLength(50);
                entity.Property(s => s.Surname).IsRequired().HasMaxLength(50);
            });

            // 2. Department Tablosu Kısıtlamaları
            modelBuilder.Entity<Department>()
                .Property(d => d.DepartmentName)
                .IsRequired()
                .HasMaxLength(100);

            // 3. LoginLog Tablosu Kısıtlamaları
            modelBuilder.Entity<LoginLog>()
                .Property(l => l.Username)
                .IsRequired();
        }
    }
}