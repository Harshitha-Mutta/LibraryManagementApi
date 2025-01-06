using LibraryManagementApi.Data;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using Microsoft.EntityFrameworkCore;


namespace LibraryManagementApi.Services
{
    public class BackgroundJobService
    {
        private readonly IBorrowedBookRepository<BorrowedBook> _borrowRepository;
        private readonly LibraryContext _context;

        public BackgroundJobService(IBorrowedBookRepository<BorrowedBook> borrowRepository, LibraryContext context)
        {
            _borrowRepository = borrowRepository;
            _context = context;
        }

        // Background job to check overdue reservations and cancel if needed
        public async Task CheckAndCancelOverduePreBookings()
        {
            // Step 1: Fetch records where BorrowDate.Date == DueDate.Date
            var borrowedBooks = await _context.BorrowedBooks
                .Where(b => b.BorrowDate.Date == b.DueDate.Date && b.ReturnDate == null)
                .ToListAsync();

            // Step 2: Apply the filtering for overdue books in-memory (client-side)
            var overdueBooks = borrowedBooks
                .Where(b => (DateTime.Now - b.BorrowDate).TotalDays > 1)
                .ToList();

            // Step 3: Call CancelPreBooking for each overdue book
            foreach (var borrowedBook in overdueBooks)
            {
                //Console.WriteLine(borrowedBook.BorrowID);
                await _borrowRepository.CancelPreBooking(borrowedBook.BookID, borrowedBook.UserID);
            }
        }

    }

}
