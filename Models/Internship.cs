namespace StajSistemi.Models
{
    public class Internship
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }

        // İlişki: Bir staj bir öğrenciye aittir
        public int StudentId { get; set; }
        public Student? Student { get; set; }
    }
}