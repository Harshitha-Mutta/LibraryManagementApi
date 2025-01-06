using LibraryManagementApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagementApi.Interfaces
{
    public interface IReservationRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(); // Get all entities
        Task<T> GetByIdAsync(int id); // Get an entity by its ID
        Task<IEnumerable<T>> GetByUserIdAsync(int userId); // Get reservations by user ID
        Task<IEnumerable<T>> GetByBookIdAsync(int bookId); // Get reservations by book ID
        Task<IEnumerable<T>> GetActiveReservationsAsync(); // Get all active reservations
        Task<IEnumerable<T>> GetCancelledReservationsAsync(); // Get all cancelled reservations
        Task<int> CountActiveReservationsAsync();
        Task<int> GetActiveReservationsCountByUserAsync(int userId);

        Task AddAsync(T entity); // Add a new entity
        Task UpdateAsync(T entity); // Update an existing entity
        Task DeleteAsync(int id); // Delete an entity by its ID
        Task<IEnumerable<T>> GetByStatusAsync(Status status);
        Task<IEnumerable<T>> GetByStatusAndUserIdAsync(Status status, int userId);
        Task CancelReservationAsync(string bookName, int userId);
        Task<List<Reservation>> GetReservationsByUserIdAsync(int userId);
        Task<IEnumerable<Reservation>> GetActiveReservationsByUserIdAsync(int userId);
        Task<IEnumerable<object>> GetUserEmailsForApprovedReservationsAsync();
        Task<Dictionary<string, int>> GetReservationStatusSummaryAsync();
    }
}