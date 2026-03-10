using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace StajSistemi.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Gmail SMTP sunucu ayarları
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                // KİMLİK DOĞRULAMA: Senin mailin ve Google'dan aldığın 16 haneli özel şifre
                Credentials = new NetworkCredential("cineksevval52@gmail.com", "pmnh zbrm tlac weae")
            };

            var mailMessage = new MailMessage
            {
                // GÖNDEREN: Sistemden mail giderken görünecek olan adres ve isim
                From = new MailAddress("cineksevval52@gmail.com", "Sinop Üni Staj Takip"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            // ALICI: Şifresini unutan kullanıcının mail adresi
            mailMessage.To.Add(email);

            // POSTACININ YOLA ÇIKMASI
            await client.SendMailAsync(mailMessage);
        }
    }
}