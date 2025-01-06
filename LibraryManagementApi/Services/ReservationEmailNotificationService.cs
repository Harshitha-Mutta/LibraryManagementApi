using LibraryManagementApi.Data;
using LibraryManagementApi.Models;
using System.Net.Mail;
using System.Net;
using LibraryManagementApi.Migrations;
using Microsoft.EntityFrameworkCore;


public class ReservationEmailNotificationService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private Timer _timer;

    public ReservationEmailNotificationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Run the task every hour (3600000 milliseconds = 1 hour)
        _timer = new Timer(CheckApprovedReservations, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(3600000));
        return Task.CompletedTask;
    }

    private void CheckApprovedReservations(object state)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();

            // Fetch approved reservations
            var reservations = context.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.ReservationStatus == Status.Approved && !context.BorrowedBooks
                    .Any(bb => bb.UserID == r.UserID && bb.BookID == r.BookID && !bb.ReturnDate.HasValue))
                .ToList();

            foreach (var reservation in reservations)
            {
                // Send an email notification
                SendEmail(reservation.User.Email, "Reservation Approved",
                          $"Your reservation for the book '{reservation.Book.Title}' has been approved. Please visit the library to borrow it.");
            }
        }
    }

    public static void SendEmail(string toEmail, string subject, string body)
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
