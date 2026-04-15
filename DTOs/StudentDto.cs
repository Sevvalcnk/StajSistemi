namespace StajSistemi.DTOs
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }

        // ✅ MÜHÜR 1: FullName property'si
        public string FullName => $"{Name} {Surname}";

        public string? StudentNo { get; set; }
        public string? Email { get; set; }
        public double? GPA { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        // 🏛️ TÜRKİYE GENELİ VİZYONU (Kayıttan kaldırıp Profile taşıdığımız alanlar)
        public string? UniversityName { get; set; }
        public string? FacultyName { get; set; }

        // 🎓 EĞİTİM DÜZEYİ MÜHÜRÜ (YENİ!)
        // "Lisans (4 Yıllık)" veya "Önlisans (2 Yıllık)" bilgisini burada tutacağız.
        public string? DegreeType { get; set; }

        // 👶 DOĞUM VE AKADEMİK BİLGİLER
        public string? BirthPlace { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? AcademicYear { get; set; }

        // 🔥 KRİTİK EKLEME: SQL'deki "Onaylandı" bilgisi
        public string? InternshipStatus { get; set; }

        public string? PhoneNumber { get; set; }
        public string? PersonalSkills { get; set; }
        public string? EducationSummary { get; set; }
        public string? CVPath { get; set; }
        public string? CertificatePath { get; set; }

        // 🛡️ MÜHÜR: Şehir Bilgileri
        public int? CityId { get; set; }
        public string? CityName { get; set; }

        // ✅ MÜHÜR 2: AdvisorId
        public int? AdvisorId { get; set; }
    }
}