using System.ComponentModel.DataAnnotations;

namespace StajSistemi.Models
{
    public class Department
    {
        [Key] // Birincil Anahtar (PK)
        public int Id { get; set; }

        [Required] // Boş geçilemez
        [Display(Name = "Bölüm Adı")]
        public string DepartmentName { get; set; }

        // İlişki: Bir bölümde birden fazla öğrenci olabilir
        public ICollection<Student>? Students { get; set; }
    }
}