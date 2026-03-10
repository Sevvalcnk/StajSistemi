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

        // --- Hata Çözümü: Bu satır Index sayfasındaki kırmızı çizgiyi silecek! ---
        public string? DepartmentName { get; set; }
    }
}