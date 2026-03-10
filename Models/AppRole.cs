using Microsoft.AspNetCore.Identity;

namespace StajSistemi.Models
{
    // Rollerimizi (Admin, Advisor, Student) yönetecek sınıf
    public class AppRole : IdentityRole<int>
    {
    }
}