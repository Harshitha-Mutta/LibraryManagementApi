using LibraryManagementApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagementApi.Interfaces
{
    public interface IBorrowedBookRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(); // Get all entities
        Task<T> GetByIdAsync(int id); // Get an entity by its ID
        Task<IEnumerable<T>> GetByUserIdAsync(int userId); // Get borrowed books by user ID
        Task<IEnumerable<T>> GetByBookIdAsync(int bookId); // Get borrowed books by book ID
        Task<IEnumerable<T>> GetActiveBorrowedBooksAsync(); // Get all active borrowed books
        Task<IEnumerable<T>> GetReturnedBorrowedBooksAsync(); // Get all returned borrowed books
        Task AddAsync(T entity); // Add a new enti
        Task UpdateAsync(T entity); // Update an existing entity
        Task DeleteAsync(int id); // Delete an entity by its ID
        Task BorrowReservedBookAsync(int userId, int bookId);
        Task<IEnumerable<T>> GetOverdueBooksAsync();
        Task<int> GetActiveBorrowedBookCountAsync();
        Task<T> GetByUserAndBookAsync(int userId, int bookId);
        Task PreBooking(int bookId, int userId);
        Task CancelPreBooking(int bookId, int userId);
        Task ApprovePreBooking(int bookId, int userId);
        Task<IEnumerable<BorrowedBooksMonthlyCount>> GetBorrowedBooksCountAsync(int userId);
        Task<IEnumerable<BorrowedBooksMonthlyCount>> GetBorrowedBooksCountForAllYearsAsync();
        Task<T> GetApprovedOrNullBorrowedBookByUserAndBookAsync(int userId, int bookId);
        Task<IEnumerable<object>> GetEmailsOfUsersWithUnreturnedBooksAsync();
        Task<IEnumerable<object>> GetAllUserEmailsAsync();
        Task<IEnumerable<object>> GetUsersWithSameBorrowAndDueDateAsync();
        Task<List<BorrowedBook>> GetBorrowedBooksByDateRangeAsync(DateTime startDate, DateTime endDate);

    }
    public class BorrowedBooksMonthlyCount
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
    }
}