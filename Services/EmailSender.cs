using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace StajSistemi.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    // Senin güncel şifren ve mailin
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
                // Bir hata olursa Visual Studio'nun 'Output' penceresinden görebilirsin.
                System.Diagnostics.Debug.WriteLine("MAİL HATASI: " + ex.Message);
            }
        }
    }
}