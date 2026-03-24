namespace StajSistemi.DTOs
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? StudentNo { get; set; }
        public string? Email { get; set; }
        public double? GPA { get; set; }
        public int? DepartmentId { get; set; }

        // ✅ Index sayfasındaki ve hoca panelindeki eşleşme için kritik
        public string? DepartmentName { get; set; }

        // 🔥 YENİ KARİYER MÜHÜRLERİ (Controller'daki hataları silen kısım)
        public string? PhoneNumber { get; set; }      // İletişim bilgisi
        public string? PersonalSkills { get; set; }   // C#, SQL gibi yetenekler
        public string? EducationSummary { get; set; } // Eğitim özeti (2. Sınıf vb.)
        public string? CVPath { get; set; }           // Kaydedilen CV'nin adresi
        public string? CertificatePath { get; set; }  // Kaydedilen Sertifikanın adresi
    }
}