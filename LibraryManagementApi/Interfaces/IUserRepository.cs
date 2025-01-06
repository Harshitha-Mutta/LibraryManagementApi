using LibraryManagementSystem.Models;
using System.Linq.Expressions;

namespace LibraryManagementApi.Interfaces
{
    public interface IUserRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();        // Get all entities
        Task<T> GetByIdAsync(int id);  // Get an entity by its ID
        Task<User> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAdminsAsync();
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByNameAsync(string name);
        Task AddAsync(T entity);        // Add a new entity
        Task UpdateAsync(T entity);        // Update an existing entity
        Task DeleteAsync(int id);        // Delete an entity by its ID
        Task UpdateBookCountAsync(int userId, int increment);
        Task<int> GetUserCountAsync();  // New method to get user count
        Task SendOtpAsync(string email);
        public bool VerifyOtp(string email, int otp);
        Task ResetPasswordAsync(string email, string newPassword);
    }
}
