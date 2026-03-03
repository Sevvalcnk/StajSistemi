namespace StajSistemi.DTOs
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? StudentNo { get; set; }
        public string? Email { get; set; } // İşte bu satır eksikti!
        public double? GPA { get; set; }
        public int? DepartmentId { get; set; }
    }
}