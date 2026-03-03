using System.ComponentModel.DataAnnotations;

namespace StajSistemi.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Kullanıcı Adı")]
        public string AdminName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}