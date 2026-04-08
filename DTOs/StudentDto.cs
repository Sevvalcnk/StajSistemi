namespace StajSistemi.DTOs
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }

        // ✅ MÜHÜR 1: FullName property'sini ekliyoruz. 
        // Bu sayede View'larda @msg.Sender.FullName dediğinde hata almayacaksın.
        public string FullName => $"{Name} {Surname}";

        public string? StudentNo { get; set; }
        public string? Email { get; set; }
        public double? GPA { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        // 🔥 KRİTİK EKLEME: SQL'deki "Onaylandı" bilgisini buraya mühürleyeceğiz
        public string? InternshipStatus { get; set; }

        public string? PhoneNumber { get; set; }
        public string? PersonalSkills { get; set; }
        public string? EducationSummary { get; set; }
        public string? CVPath { get; set; }
        public string? CertificatePath { get; set; }

        // 🛡️ MÜHÜR: Sayfadan gelen Şehir ID'sini bu kutu taşıyacak
        public int? CityId { get; set; }
        // 🛡️ MÜHÜR: Şehrin ID'si yetmez, adını da taşımamız lazım
        public string? CityName { get; set; }

        // ✅ MÜHÜR 2: İşte o aradığımız "AdvisorId" burası!
        // Öğrencinin danışmanına soru sorabilmesi için bu veriyi taşımamız şart.
        public int? AdvisorId { get; set; }
    }
}