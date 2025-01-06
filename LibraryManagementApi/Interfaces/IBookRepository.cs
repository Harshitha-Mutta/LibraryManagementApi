using LibraryManagementApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagementApi.Interfaces
{
    public interface IBookRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(); // Get all entities
        Task<T> GetByIdAsync(int id); // Get an entity by its ID
        Task AddAsync(T entity); // Add a new entity
        Task UpdateAsync(T entity); // Update an existing entity
        Task DeleteAsync(int id); // Delete an entity by its ID
        Task UpdateAvailableCopiesAsync(int bookId, int Increment);
        Task<T> GetBookByTitleAsync(string title);
        Task<IEnumerable<Book>> GetNotAvailableBooksAsync();
        Task<IEnumerable<Book>> GetAvailableBooksAsync();
        Task<IEnumerable<Book>> SearchBooksAsync(string genreName, string author, string language, string publisherName, string title);
        Task<int> CalculateTotalBooksAsync();
        Task<List<string>> GetSearchSuggestionsAsync(string field, string query);


    }
}
