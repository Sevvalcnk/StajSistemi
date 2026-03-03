namespace StajSistemi.Models
{
    public class LoginLog
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public DateTime LoginTime { get; set; } = DateTime.Now;
        public string? IpAddress { get; set; }
    }
}