namespace StajSistemi.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string? Name { get; set; }      // Sonuna ? ekledik
        public string? Surname { get; set; }   // Sonuna ? ekledik
        public string? StudentNo { get; set; } // Sonuna ? ekledik
        public double? GPA { get; set; }
        public string? Email { get; set; }
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; } // Bu satır olmadan eşleme yapamazsın!
    }
}