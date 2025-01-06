using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagementApi.Interfaces
{
    public interface IGenreRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<int> GetGenreCountAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }
}
