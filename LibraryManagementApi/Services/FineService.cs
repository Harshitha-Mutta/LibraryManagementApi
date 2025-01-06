using LibraryManagementApi.Data;
using LibraryManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementApi.Services
{
    public class FineService
    {
        private readonly LibraryContext _context;

        public FineService(LibraryContext context)
        {
            _context = context;
        }

        public async Task GenerateFinesForOverdueBooksAsync()
        {
            try
            {
                // Get all borrowed books that are overdue and not yet returned
                var overdueBooks = await _context.BorrowedBooks
                    .Where(bb => bb.DueDate < DateTime.Now && bb.ReturnDate == null && (bb.BorrowStatus == null || bb.BorrowStatus == BorrowStatus.BookingAllocated))
                    .ToListAsync();

                foreach (var overdueBook in overdueBooks)
                {
                    // Check if a fine already exists for this user and book
                    var existingFine = await _context.Fines
                        .FirstOrDefaultAsync(f =>
                            f.UserID == overdueBook.UserID && f.BookID == overdueBook.BookID && f.PaidStatus == FineStatus.NotPaid);

                    if (existingFine == null && (DateTime.Now - overdueBook.DueDate).Days > 0)
                    {
                        // Calculate the fine amount, e.g., $5 per day overdue
                        var daysOverdue = (DateTime.Now - overdueBook.DueDate).Days;
                        decimal fineAmount = daysOverdue * 5.0m; // Example fine amount of 5 units per day

                        // Create a new fine record
                        var fine = new Fine
                        {
                            UserID = overdueBook.UserID,
                            BookID = overdueBook.BookID,
                            Amount = fineAmount,
                            FineDate = DateTime.Now,
                            PaidStatus = FineStatus.NotPaid
                        };

                        await _context.Fines.AddAsync(fine);
                    }
                    else if (existingFine != null)
                    {
                        // Calculate additional fine amount for another overdue day
                        var daysOverdue = (DateTime.Now - overdueBook.DueDate).Days;
                        existingFine.Amount = daysOverdue * 5.0m; // Update fine to reflect the new overdue days
                        _context.Fines.Update(existingFine);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (assuming you have a logger in place)
                // log.Error("An error occurred while generating fines for overdue books.", ex);
                throw new Exception("An error occurred while generating fines for overdue books.", ex);
            }
        }
    }
}