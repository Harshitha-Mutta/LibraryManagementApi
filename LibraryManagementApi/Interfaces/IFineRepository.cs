using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;

namespace LibraryManagementApi.Interfaces
{
    public interface IFineRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(); // Get all entities
        Task<T> GetByIdAsync(int id); // Get an entity by its ID
        Task<IEnumerable<T>> GetByUserIdAsync(int userId); // Get fines by user ID
        Task<decimal> GetTotalUnpaidFinesByUserIdAsync(int userId);
        Task<decimal> GetTotalUnpaidFinesAsync();
        Task UpdateAllUnpaidFinesToPaidByUserIdAsync(int userId);
        Task AddAsync(T entity); // Add a new entity
        Task UpdateAsync(T entity); // Update an existing entity
        Task DeleteAsync(int id); // Delete an entity by its ID
        Task<IEnumerable<T>> GetFinesByStatusAsync(FineStatus status);
        Task<IEnumerable<T>> GetFinesByUserIdAndStatusAsync(int userId, FineStatus status);
        Task<List<Fine>> GetUnpaidFinesAsync();



    }
}
