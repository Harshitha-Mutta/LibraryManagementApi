using LibraryManagementApi.Data;
using LibraryManagementApi.Models;
using System.Net.Mail;
using System.Net;
using LibraryManagementApi.Migrations;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Services
{
    public class BorrowedBookEmailService 
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public BorrowedBookEmailService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        
    
        public void SendEmailWithPdf(string toEmail, string subject, string body, string pdfPath = null)
        {
            string fromEmail = "kani2002shkka@gmail.com";
            string password = "cgqsxdvfmtqimunq";

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromEmail);
            mailMessage.To.Add(toEmail);
            mailMessage.Subject = subject;
            mailMessage.Body = "Borrowed Books Details Pdf will be attached below kindly have a glance";
            mailMessage.IsBodyHtml = true;
            if (!string.IsNullOrEmpty(pdfPath) && File.Exists(pdfPath))
            {
                mailMessage.Attachments.Add(new Attachment(pdfPath));
            }

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            smtpClient.Send(mailMessage);
        }
    }
}
