using AutoMapper;
using StajSistemi.Models; // Entity sınıfların için
using StajSistemi.DTOs;   // DTO sınıfların için

namespace StajSistemi.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            // ReverseMap() sayesinde çift yönlü dönüşüm yapabiliriz.
            // Yani hem Student'ı StudentDto'ya, hem de tam tersine dönüştürebilir.
            CreateMap<Student, StudentDto>().ReverseMap();
            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<Advisor, AdvisorDto>().ReverseMap();
            CreateMap<Admin, AdminDto>().ReverseMap();
        }
    }
}