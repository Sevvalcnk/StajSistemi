namespace StajSistemi.Models
{
    public class Application
    {
        public int Id { get; set; }
        public DateTime ApplicationDate { get; set; } = DateTime.Now;
        public string? Status { get; set; } // Onaylandı, Beklemede, Reddedildi

        public int StudentId { get; set; }
        public Student? Student { get; set; }
    }
}