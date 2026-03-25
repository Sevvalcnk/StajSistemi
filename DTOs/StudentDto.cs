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
        public string? DepartmentName { get; set; }

        // 🔥 KRİTİK EKLEME: SQL'deki "Onaylandı" bilgisini buraya mühürleyeceğiz
        public string? InternshipStatus { get; set; }

        public string? PhoneNumber { get; set; }
        public string? PersonalSkills { get; set; }
        public string? EducationSummary { get; set; }
        public string? CVPath { get; set; }
        public string? CertificatePath { get; set; }
    }
}