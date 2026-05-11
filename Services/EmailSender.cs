using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.IO; // 🚀 DOSYA İŞLEMLERİ İÇİN ŞART

namespace StajSistemi.Services
{
    public class EmailSender : IEmailSender
    {
        // 1. STANDART METOT (ASLA BOZULMADI - ESKİ YERLER ÇALIŞMAYA DEVAM EDER)
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("cineksevval52@gmail.com", "iztu jove babo vdwm")
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("cineksevval52@gmail.com", "Sinop Üni Staj Takip"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MAİL HATASI: " + ex.Message);
            }
        }

        // 🚀 2. YENİ SİBER METOT: DOSYA EKLİ MAİL GÖNDERME
        // AdminController'dan PDF gönderirken bu metodu çağıracağız.
        public async Task SendEmailWithAttachmentAsync(string email, string subject, string htmlMessage, byte[] attachment, string fileName)
        {
            try
            {
                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("cineksevval52@gmail.com", "iztu jove babo vdwm")
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("cineksevval52@gmail.com", "Sinop Üni Staj Takip"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                // 🛡️ PAKETİ ZARFA KOYMA (PDF EKLEME)
                if (attachment != null)
                {
                    mailMessage.Attachments.Add(new Attachment(new MemoryStream(attachment), fileName));
                }

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EKLİ MAİL HATASI: " + ex.Message);
            }
        }
    }
}