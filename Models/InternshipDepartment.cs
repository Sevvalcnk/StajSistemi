namespace StajSistemi.Models
{
    public class InternshipDepartment
    {
        // 🆔 İlan Kimliği
        public int InternshipId { get; set; }
        public Internship Internship { get; set; }

        // 🆔 Bölüm Kimliği
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
    }
}