using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using LibraryManagementApi.Models;
using LibraryManagementApi.Migrations;
using LibraryManagementApi.Data;
using Microsoft.EntityFrameworkCore;
public class EmailNotificationService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private Timer _timer;
    private readonly LibraryContext _libraryContext;

    public EmailNotificationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Run the task every day (86400000 milliseconds = 1 day)
        _timer = new Timer(SendDueDateEmails, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(86400000));
        return Task.CompletedTask;
    }

    private void SendDueDateEmails(object state)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryContext>(); // Your DbContext

            var today = DateTime.Now;

            // Fetch users with overdue borrowed books
            var overdueBooks = context.BorrowedBooks
                .Where(b => b.DueDate < today && b.ReturnDate == null &&b.BorrowStatus!=BorrowStatus.Cancelled)
                .Select(b => new { b.UserID, b.DueDate, b.User.Email })
                .ToList();

            foreach (var book in overdueBooks)
            {
                SendEmail(book.Email, "Overdue Notification", $"This email is notify for the overDue. Please return the book in the library office. Your book was due on {book.DueDate.ToShortDateString()}.");
            }
        }
    }
    
    public void SendEmail(string toEmail, string subject, string body)
    {
        string fromEmail = "kani2002shkka@gmail.com";
        string password = "cgqsxdvfmtqimunq";

        MailMessage mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(fromEmail);
        mailMessage.To.Add(toEmail);
        mailMessage.Subject = subject;
        mailMessage.Body = body;
        mailMessage.IsBodyHtml = true;
        

        SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl = true
        };

        smtpClient.Send(mailMessage);
    }
    

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}