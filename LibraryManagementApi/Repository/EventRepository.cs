using LibraryManagementApi.Data;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementApi.Repository
{
    public class EventRepository : IEventRepository<Event>
    {
        private readonly LibraryContext _context;

        public EventRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _context.Events.ToListAsync();
        }

        public async Task<Event> GetEventByIdAsync(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                throw new Exception($"Event with ID {id} not found.");
            }

            return eventItem;
        }

        public async Task AddEventAsync(Event newEvent)
        {
            await _context.Events.AddAsync(newEvent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEventAsync(Event updatedEvent)
        {
            _context.Entry(updatedEvent).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEventAsync(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                throw new Exception($"Event with ID {id} not found.");
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            return await _context.Events
                .Where(e => e.EventDate > DateTime.Now)
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetPastEventsAsync()
        {
            return await _context.Events
                .Where(e => e.EventDate <= DateTime.Now)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> SearchEventsAsync(string? eventName, DateTime? eventDate, string? description)
        {
            var query = _context.Events.AsQueryable();

            if (!string.IsNullOrEmpty(eventName))
            {
                query = query.Where(e => e.EventName.Contains(eventName));
            }

            if (eventDate.HasValue)
            {
                query = query.Where(e => e.EventDate.Date == eventDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(description))
            {
                query = query.Where(e => e.Description.Contains(description));
            }

            return await query.ToListAsync();
        }

        public async Task<int> CalculateTotalEventsAsync()
        {
            return await _context.Events.CountAsync();
        }
    }
}
