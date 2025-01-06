using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagementApi.Models;

namespace LibraryManagementApi.Interfaces
{
    public interface IEventRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllEventsAsync();
        Task<T> GetEventByIdAsync(int id);
        Task AddEventAsync(T eventItem);//admin
        Task UpdateEventAsync(T eventItem);//admin
        Task DeleteEventAsync(int id);//admin
        Task<IEnumerable<T>> GetUpcomingEventsAsync(); //user
        Task<IEnumerable<T>> GetPastEventsAsync(); //admin
        Task<IEnumerable<T>> SearchEventsAsync(string? eventName, DateTime? eventDate, string? description);
        Task<int> CalculateTotalEventsAsync(); //admin
    }

}
