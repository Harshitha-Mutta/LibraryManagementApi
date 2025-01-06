using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using LibraryManagementApi.Exceptions;
using LibraryManagementApi.Repository;

namespace LibraryManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository<Event> _eventRepository;
        private static readonly ILog log = LogManager.GetLogger(typeof(EventController));

        public EventController(IEventRepository<Event> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        // GET: api/Event
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            log.Info("Fetching all events...");
            var events = await _eventRepository.GetAllEventsAsync();
            if (events == null || !events.Any())
            {
                log.Warn("No events found");
                return NotFound();
            }
            log.Info("Events retrieved successfully");
            return Ok(events);
        }

        // GET: api/Event/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            log.Info($"Fetching event with ID: {id}");
            var eventItem = await _eventRepository.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                log.Warn($"Event with ID {id} not found");
                return NotFound();
            }
            log.Info($"Event with ID {id} retrieved successfully");
            return Ok(eventItem);
        }

        // POST: api/Event
        [HttpPost]
        public async Task<IActionResult> AddEvent([FromBody] Event newEvent)
        {
            log.Info("Adding a new event...");
            if (!ModelState.IsValid)
            {
                log.Warn("Invalid event data");
                return BadRequest(ModelState);
            }

            await _eventRepository.AddEventAsync(newEvent);
            log.Info($"Event '{newEvent.EventName}' added successfully");
            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventID }, newEvent);
        }

        // PUT: api/Event/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updatedEvent)
        {
            log.Info($"Updating event with ID: {id}");
            if (id != updatedEvent.EventID)
            {
                log.Warn("Event ID mismatch");
                return BadRequest("Event ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                log.Warn("Invalid event data");
                return BadRequest(ModelState);
            }

            await _eventRepository.UpdateEventAsync(updatedEvent);
            log.Info($"Event with ID {id} updated successfully");
            return NoContent();
        }

        // DELETE: api/Event/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            log.Info($"Deleting event with ID: {id}");
            var eventItem = await _eventRepository.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                log.Warn($"Event with ID {id} not found");
                return NotFound();
            }

            await _eventRepository.DeleteEventAsync(id);
            log.Info($"Event with ID {id} deleted successfully");
            return NoContent();
        }

        // GET: api/Event/upcoming
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingEvents()
        {
            log.Info("Fetching upcoming events...");
            var events = await _eventRepository.GetUpcomingEventsAsync();
            if (events == null || !events.Any())
            {
                log.Warn("No upcoming events found");
                return NotFound();
            }
            log.Info("Upcoming events retrieved successfully");
            return Ok(events);
        }

        // GET: api/Event/past
        [HttpGet("past")]
        public async Task<IActionResult> GetPastEvents()
        {
            log.Info("Fetching past events...");
            var events = await _eventRepository.GetPastEventsAsync();
            if (events == null || !events.Any())
            {
                log.Warn("No past events found");
                return NotFound();
            }
            log.Info("Past events retrieved successfully");
            return Ok(events);
        }

        // GET: api/Event/search
        [HttpGet("search")]
        public async Task<IActionResult> SearchEvents(
            [FromQuery] string? eventName,
            [FromQuery] DateTime? eventDate,
            [FromQuery] string? description)
        {
            try
            {
                var events = await _eventRepository.SearchEventsAsync(eventName, eventDate, description);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Event/total
        [HttpGet("total")]
        public async Task<IActionResult> GetTotalEvents()
        {
            try
            {
                var totalEvents = await _eventRepository.CalculateTotalEventsAsync();
                return Ok(new { totalEvents });
            }
            catch (EventException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
