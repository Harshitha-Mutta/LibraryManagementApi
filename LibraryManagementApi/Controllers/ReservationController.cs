using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LibraryManagementApi.Dtos;
using log4net;
using LibraryManagementApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagementApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationRepository<Reservation> _reservationRepository;
        private static readonly ILog log = LogManager.GetLogger(typeof(ReservationController));
        private readonly LibraryContext _context;

        public ReservationController(IReservationRepository<Reservation> reservationRepository, LibraryContext context)

        {
            _reservationRepository = reservationRepository;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            log.Info("Fetching all reservations...");

            // Fetch all reservations, including user and book details
            var reservations = await _reservationRepository.GetAllAsync();

            // Create an anonymous type to shape the data
            var result = reservations.Select(r => new
            {
                r.ReservationID,
                UserName = r.User != null ? r.User.Name : "N/A", // Safely access UserName
                BookTitle = r.Book != null ? r.Book.Title : "N/A", // Safely access Book Title
                r.ReservationDate,
                r.ReservationStatus,
                r.QueuePosition
            });

            if (!result.Any())
            {
                log.Warn("No reservations found.");
                return NotFound("No reservations found.");
            }

            log.Info("Reservations fetched successfully");
            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(int id)
        {
            log.Info($"Fetching reservation with ID: {id}");
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                log.Warn($"Reservation with ID {id} not found");
                return NotFound();
            }
            log.Info($"Reservation with ID {id} fetched successfully");
            return Ok(reservation);
        }

        [HttpPost]
        public async Task<IActionResult> AddReservation([FromBody] ReservationCreateDto reservationDto)
        {
            log.Info("Adding a new reservation...");

            if (!ModelState.IsValid)
            {
                log.Warn("Invalid reservation data");
                return BadRequest(ModelState);
            }

            // Use the injected DbContext to find the book by its name
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Title == reservationDto.BookName);

            if (book == null)
            {
                log.Warn("Book not found");
                return BadRequest(new { message = "Book not found." });
            }

            var reservation = new Reservation
            {
                UserID = reservationDto.UserID,
                BookID = book.BookID, // Use the found BookID
                ReservationDate = DateTime.Now,
                ReservationStatus = Status.Active
            };

            try
            {
                // Use repository for adding the reservation
                await _reservationRepository.AddAsync(reservation);
                return Ok("Reservation added successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle other possible errors
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationUpdateDto reservationDto)
        {
            log.Info($"Updating reservation with ID: {id}");

            if (id != reservationDto.ReservationID)
            {
                log.Warn("Reservation ID mismatch");
                return BadRequest();
            }

            // Retrieve the existing reservation from the database
            var existingReservation = await _reservationRepository.GetByIdAsync(id);
            if (existingReservation == null)
            {
                log.Warn($"Reservation with ID {id} not found");
                return NotFound();
            }

            // Map the DTO to the Reservation entity
            existingReservation.UserID = reservationDto.UserID;
            existingReservation.BookID = reservationDto.BookID;
            existingReservation.ReservationStatus = reservationDto.ReservationStatus;

            // Update the reservation in the repository
            await _reservationRepository.UpdateAsync(existingReservation);
            log.Info($"Reservation with ID {id} updated successfully");

            return NoContent();
        }

        [HttpGet("active/count")]
        public async Task<IActionResult> GetActiveReservationCount()
        {
            log.Info("Fetching count of active reservations...");
            var count = await _reservationRepository.CountActiveReservationsAsync();
            log.Info($"Active reservations count: {count}");
            return Ok(new { ActiveReservationsCount = count });
        }

        [HttpGet("active-reservations")]
        public async Task<IActionResult> GetActiveReservationsByUserCount(int userId)
        {
            log.Info($"Fetching active reservations count for user with ID: {userId}");
            var count = await _reservationRepository.GetActiveReservationsCountByUserAsync(userId);
            log.Info($"Active reservations count for user with ID {userId}: {count}");
            return Ok(new { ActiveReservationsCount = count });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            log.Info($"Deleting reservation with ID: {id}");
            await _reservationRepository.DeleteAsync(id);
            log.Info($"Reservation with ID {id} deleted successfully");
            return NoContent();
        }
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetReservationsByStatus(Status status)
        {
            log.Info($"Fetching reservations with status: {status}");

            var reservations = await _reservationRepository.GetByStatusAsync(status);

            var result = reservations.Select(r => new
            {
                r.ReservationID,
                UserName = r.User != null ? r.User.Name : "N/A",
                BookTitle = r.Book != null ? r.Book.Title : "N/A",
                r.ReservationDate,
                r.ReservationStatus,
                r.QueuePosition
            });

            if (!result.Any())
            {
                log.Warn("No reservations found.");
                return NotFound("No reservations found.");
            }

            log.Info("Reservations fetched successfully");
            return Ok(result);
        }

        [HttpGet("status/{status}/user/{userId}")]
        public async Task<IActionResult> GetReservationsByStatusAndUserId(Status status, int userId)
        {
            log.Info($"Fetching reservations with status: {status} for user ID: {userId}");

            var reservations = await _reservationRepository.GetByStatusAndUserIdAsync(status, userId);

            if (!reservations.Any())
            {
                log.Warn($"No reservations found with status {status} for user with ID {userId}.");
                return NotFound(new { message = $"No reservations found with status {status} for user with ID {userId}." });
            }

            // Create an anonymous type to shape the data
            var result = reservations.Select(r => new
            {
                r.ReservationID,
                UserName = r.User != null ? r.User.Name : "N/A", // Safely access UserName
                BookTitle = r.Book != null ? r.Book.Title : "N/A", // Safely access Book Title
                r.ReservationDate,
                r.ReservationStatus,
                r.QueuePosition
            });

            log.Info("Reservations fetched successfully");
            return Ok(result);
        }
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelReservation(string BookName, int UserID)
        {
            try
            {
                await _reservationRepository.CancelReservationAsync(BookName, UserID);
                return Ok("Reservation cancelled successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReservationsByUserId(int userId)
        {
            var reservations = await _reservationRepository.GetReservationsByUserIdAsync(userId);

            if (!reservations.Any())
            {
                return NotFound(new { message = $"No reservations found for user with ID {userId}." });
            }

            // Optionally, project the result into an anonymous type to shape the data as needed
            var result = reservations.Select(r => new
            {
                r.ReservationID,
                UserName = r.User != null ? r.User.Name : "N/A",  // Safely access User Name
                BookTitle = r.Book != null ? r.Book.Title : "N/A",  // Safely access Book Title
                r.ReservationDate,
                r.ReservationStatus,
                r.QueuePosition
            });

            return Ok(result);
        }
        [HttpGet("active/{userId}")]
        public async Task<IActionResult> GetActiveReservations(int userId)
        {
            log.Info($"Fetching active reservations for user ID: {userId}");

            // Call the repository method to get active reservations
            var reservations = await _reservationRepository.GetActiveReservationsByUserIdAsync(userId);

            if (reservations == null || !reservations.Any())
            {
                log.Warn($"No active reservations found for user ID: {userId}");
                return NotFound("No active reservations found.");
            }

            log.Info("Active reservations retrieved successfully");
            var result = reservations.Select(r => new
            {
                r.ReservationID,
                UserName = r.User != null ? r.User.Name : "N/A",  // Safely access User Name
                BookTitle = r.Book != null ? r.Book.Title : "N/A",  // Safely access Book Title
                r.ReservationDate,
                r.ReservationStatus,
                r.QueuePosition
            });

            return Ok(result);
        }
        [HttpGet("ApprovedEmails")]
        public async Task<ActionResult<IEnumerable<object>>> GetApprovedReservationEmails()
        {
            try
            {
                // Call the repository method to get UserID and Email of users with approved reservations
                var emails = await _reservationRepository.GetUserEmailsForApprovedReservationsAsync();

                // Check if no emails are found
                if (emails == null || !emails.Any())
                {
                    return NotFound("No approved reservations found.");
                }

                // Return the list of UserIDs and Emails
                return Ok(emails);
            }
            catch (Exception ex)
            {
                // Log the exception if you have a logging service (optional)
                // _logger.LogError(ex, "Error retrieving emails for approved reservations.");

                return StatusCode(500, "An error occurred while retrieving emails for approved reservations.");
            }
        }
        [HttpGet("status-summary")]
        public async Task<IActionResult> GetReservationStatusSummary()
        {
            var statusSummary = await _reservationRepository.GetReservationStatusSummaryAsync();
            return Ok(statusSummary);
        }


    }
}