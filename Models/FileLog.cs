using System;

namespace StajSistemi.Models
{
    public class FileLog
    {
        public int Id { get; set; }
        public int AppUserId { get; set; } // Hangi öğrenci yükledi?
        public string? FileName { get; set; } // Dosyanın adı ne?
        public string? FileType { get; set; } // 'CV' mi yoksa 'Sertifika' mı?
        public DateTime UploadTime { get; set; } = DateTime.Now; // Ne zaman yüklendi?
        public string? IpAddress { get; set; } // Hangi IP'den yüklendi?
    }
}