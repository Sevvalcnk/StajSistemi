using System.ComponentModel.DataAnnotations;

namespace StajSistemi.Models
{
    public class Advisor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ad")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        public string Surname { get; set; }

        [Required]
        [EmailAddress] // Email formatı kontrolü
        public string Email { get; set; }
    }
}