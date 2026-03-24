using AutoMapper;
using StajSistemi.Models;
using StajSistemi.DTOs;

namespace StajSistemi.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            // ✅ 1. ÖĞRENCİ MÜHÜRÜ: AppUser -> StudentDto
            CreateMap<AppUser, StudentDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src =>
                    src.Department != null ? src.Department.DepartmentName : "Bölüm Belirtilmemiş"))
                .ReverseMap()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Surname));

            // ✅ 2. DANIŞMAN MÜHÜRÜ: AppUser -> AdvisorDto
            // Artık Advisor modeli yok, her şey AppUser üzerinden eşleşiyor
            CreateMap<AppUser, AdvisorDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.LastName))
                .ReverseMap()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Surname));

            // ✅ 3. ADMİN MÜHÜRÜ: AppUser -> AdminDto
            CreateMap<AppUser, AdminDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.LastName))
                .ReverseMap()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Surname));

            // ✅ 4. DİĞER TABLOLAR
            CreateMap<Department, DepartmentDto>().ReverseMap();
        }
    }
}