using LibraryManagementApi.Dtos;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using log4net;
using Microsoft.EntityFrameworkCore;
using LibraryManagementApi.Repository;

namespace LibraryManagementApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowedBookController : ControllerBase
    {
        private readonly IBorrowedBookRepository<BorrowedBook> _borrowedBookRepository;
        private static readonly ILog log = LogManager.GetLogger(typeof(BorrowedBookController));

        public BorrowedBookController(IBorrowedBookRepository<BorrowedBook> borrowedBookRepository)
        {
            _borrowedBookRepository = borrowedBookRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBorrowedBooks()
        {
            log.Info("Fetching all borrowed books...");
            var borrowedBooks = await _borrowedBookRepository.GetAllAsync();
            log.Info("Borrowed books fetched successfully");
            var result = borrowedBooks.Select(b => new
            {
                b.BorrowID,                           // Keep the original BorrowID
                UserName = b.User != null ? b.User.Name : "N/A",            // Get the UserName from the User navigation property
                BookTitle = b.Book != null ? b.Book.Title : "N/A",              // Get the Title from the Book navigation property
                b.BorrowDate,                         // Keep the original BorrowDate
                b.DueDate,                            // Keep the original DueDate
                b.ReturnDate,                         // Keep the original ReturnDate
                BorrowStatus = b.BorrowStatus.ToString() // Convert BorrowStatus enum to string if necessary
            });
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBorrowedBookById(int id)
        {
            log.Info($"Fetching borrowed book with ID: {id}");
            var borrowedBook = await _borrowedBookRepository.GetByIdAsync(id);
            if (borrowedBook == null)
            {
                log.Warn($"Borrowed book with ID {id} not found");
                return NotFound();
            }
            log.Info($"Borrowed book with ID {id} fetched successfully");
            return Ok(borrowedBook);
        }

        [HttpPost]
        public async Task<IActionResult> AddBorrowedBook(BorrowedBookCreateDto borrowedBookDto)
        {
            log.Info("Adding a new borrowed book...");
            if (!ModelState.IsValid)
            {
                log.Warn("Invalid borrowed book data");
                return BadRequest(ModelState);
            }

            var borrowedBook = new BorrowedBook
            {
                UserID = borrowedBookDto.UserID,
                BookID = borrowedBookDto.BookID,
                BorrowDate = borrowedBookDto.BorrowDate
            };

            await _borrowedBookRepository.AddAsync(borrowedBook);
            log.Info($"Borrowed book added successfully with ID: {borrowedBook.BorrowID}");
            return CreatedAtAction(nameof(GetBorrowedBookById), new { id = borrowedBook.BorrowID }, borrowedBook);
        }

        [HttpPut("ReturnBorrowedBook")]
        public async Task<IActionResult> ReturnBorrowedBook([FromBody] BorrowedBookUpdateDto borrowedBookDto)
        {
            log.Info($"Updating borrowed book with User ID: {borrowedBookDto.UserID} and Book ID: {borrowedBookDto.BookID}");

            if (!ModelState.IsValid)
            {
                log.Warn("Invalid model state");
                return BadRequest(ModelState);
            }

            // Check if the provided ID matches the DTO's UserID and BookID (optional check for consistency)


            // Retrieve the existing BorrowedBook entity from the repository
            var existingBorrowedBook = await _borrowedBookRepository.GetApprovedOrNullBorrowedBookByUserAndBookAsync(borrowedBookDto.UserID, borrowedBookDto.BookID);

            if (existingBorrowedBook == null)
            {
                log.Warn($"Borrowed book with User ID: {borrowedBookDto.UserID} and Book ID: {borrowedBookDto.BookID} not found");
                return NotFound();
            }

            // Update the fields of the existing BorrowedBook entity using the DTO
            existingBorrowedBook.UserID = borrowedBookDto.UserID;
            existingBorrowedBook.BookID = borrowedBookDto.BookID;
            existingBorrowedBook.ReturnDate = borrowedBookDto.ReturnDate;

            // Call the repository to update the BorrowedBook entity in the database
            await _borrowedBookRepository.UpdateAsync(existingBorrowedBook);

            log.Info($"Borrowed book with User ID: {borrowedBookDto.UserID} and Book ID: {borrowedBookDto.BookID} updated successfully");

            return NoContent();

        }
        [HttpGet("active/count")]
        public async Task<IActionResult> GetActiveBorrowedBookCount()
        {
            var activeBorrowedBookCount = await _borrowedBookRepository.GetActiveBorrowedBookCountAsync();
            return Ok(activeBorrowedBookCount);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBorrowedBook(int id)
        {
            log.Info($"Deleting borrowed book with ID: {id}");
            await _borrowedBookRepository.DeleteAsync(id);
            log.Info($"Borrowed book with ID {id} deleted successfully");
            return NoContent();
        }
        [HttpPost("borrow-reserved-book")]
        public async Task<IActionResult> BorrowReservedBookAsync([FromBody] BorrowReservedBookRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Request data is null.");
            }

            try
            {
                await _borrowedBookRepository.BorrowReservedBookAsync(request.UserID, request.BookID);
                return Ok("Book borrowed successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueBooks()
        {
            try
            {
                var overdueBooks = await _borrowedBookRepository.GetOverdueBooksAsync();
                var result = overdueBooks.Select(b => new
                {
                    b.BorrowID,                           // Keep the original BorrowID
                    UserName = b.User != null ? b.User.Name : "N/A",            // Get the UserName from the User navigation property
                    BookTitle = b.Book != null ? b.Book.Title : "N/A",              // Get the Title from the Book navigation property
                    b.BorrowDate,                         // Keep the original BorrowDate
                    b.DueDate,                            // Keep the original DueDate
                    b.ReturnDate,                         // Keep the original ReturnDate
                    BorrowStatus = b.BorrowStatus.ToString() // Convert BorrowStatus enum to string if necessary
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/BorrowedBook/count


        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<BorrowedBook>>> GetByUserId(int userId)
        {
            var borrowedBooks = await _borrowedBookRepository.GetByUserIdAsync(userId);
            if (borrowedBooks == null || !borrowedBooks.Any())
            {
                return NotFound($"No borrowed books found for user ID {userId}.");
            }
            var result = borrowedBooks.Select(b => new
            {
                b.BorrowID,                           // Keep the original BorrowID
                UserName = b.User != null ? b.User.Name : "N/A",            // Get the UserName from the User navigation property
                BookTitle = b.Book != null ? b.Book.Title : "N/A",              // Get the Title from the Book navigation property
                b.BorrowDate,                         // Keep the original BorrowDate
                b.DueDate,                            // Keep the original DueDate
                b.ReturnDate,                         // Keep the original ReturnDate
                BorrowStatus = b.BorrowStatus.ToString() // Convert BorrowStatus enum to string if necessary
            });
            return Ok(result);
        }

        // GET api/borrowedbooks/book/{bookId}
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<IEnumerable<BorrowedBook>>> GetByBookId(int bookId)
        {
            var borrowedBooks = await _borrowedBookRepository.GetByBookIdAsync(bookId);
            if (borrowedBooks == null || !borrowedBooks.Any())
            {
                return NotFound($"No borrowed books found for book ID {bookId}.");
            }
            var result = borrowedBooks.Select(b => new
            {
                b.BorrowID,                           // Keep the original BorrowID
                UserName = b.User != null ? b.User.Name : "N/A",            // Get the UserName from the User navigation property
                BookTitle = b.Book != null ? b.Book.Title : "N/A",              // Get the Title from the Book navigation property
                b.BorrowDate,                         // Keep the original BorrowDate
                b.DueDate,                            // Keep the original DueDate
                b.ReturnDate,                         // Keep the original ReturnDate
                BorrowStatus = b.BorrowStatus.ToString() // Convert BorrowStatus enum to string if necessary
            });
            return Ok(result);
        }
        [HttpPost("prebooking")]
        public async Task<IActionResult> PreBooking(int bookId, int userId)
        {
            try
            {
                await _borrowedBookRepository.PreBooking(bookId, userId);
                return Ok("Pre-booking successful.");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        [HttpPost("cancel-prebooking")]
        public async Task<IActionResult> CancelPreBooking(int bookId, int userId)
        {
            try
            {
                await _borrowedBookRepository.CancelPreBooking(bookId, userId);
                return Ok("Pre-booking canceled successfully.");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        [HttpPost("approve-prebooking")]
        public async Task<IActionResult> ApprovePreBooking(int bookId, int userId)
        {
            try
            {
                await _borrowedBookRepository.ApprovePreBooking(bookId, userId);
                return Ok("Pre-booking approved successfully.");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        [HttpGet("borrowed-count")]
        public async Task<IActionResult> GetBorrowedBooksCount([FromQuery] int userId)
        {
            var result = await _borrowedBookRepository.GetBorrowedBooksCountAsync(userId);
            return Ok(result);
        }
        [HttpGet("borrowed-books/monthly-count/all-years")]
        public async Task<IActionResult> GetBorrowedBooksCountForAllYears()
        {
            var monthlyCounts = await _borrowedBookRepository.GetBorrowedBooksCountForAllYearsAsync();

            if (monthlyCounts == null || !monthlyCounts.Any())
            {
                return NotFound("No borrowed books data available.");
            }

            return Ok(monthlyCounts);
        }
        [HttpGet("UnreturnedBooks/Emails")]
        public async Task<ActionResult<IEnumerable<object>>> GetEmailsOfUsersWithUnreturnedBooks()
        {
            try
            {
                // Call the repository method to get the emails
                var userEmails = await _borrowedBookRepository.GetEmailsOfUsersWithUnreturnedBooksAsync();

                // Check if no emails are found
                if (userEmails == null || !userEmails.Any())
                {
                    return NotFound("No users with unreturned books found.");
                }

                // Return the list of user emails
                return Ok(userEmails);
            }
            catch (Exception ex)
            {
                // Log the exception if you have a logging service (optional)
                // _logger.LogError(ex, "Error retrieving emails of users with unreturned books.");

                return StatusCode(500, "An error occurred while retrieving emails of users with unreturned books.");
            }
        }
        [HttpGet("SameDate")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsersWithSameBorrowAndDueDate()
        {
            try
            {
                // Call the repository method to get UserID and Email of users with same BorrowDate and DueDate
                var users = await _borrowedBookRepository.GetUsersWithSameBorrowAndDueDateAsync();

                // Check if no users are found
                if (users == null || !users.Any())
                {
                    return NotFound("No users found with the same BorrowDate and DueDate.");
                }

                // Return the list of UserIDs and Emails
                return Ok(users);
            }
            catch (Exception ex)
            {
                // Log the exception if you have a logging service (optional)
                // _logger.LogError(ex, "Error retrieving users with the same BorrowDate and DueDate.");

                return StatusCode(500, "An error occurred while retrieving users with the same BorrowDate and DueDate.");
            }
        }

        // GET api/Users/AllEmails
        [HttpGet("AllEmails")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUserEmails()
        {
            try
            {
                // Call the repository method to get UserID and Email of all users
                var users = await _borrowedBookRepository.GetAllUserEmailsAsync();

                // Check if no users are found
                if (users == null || !users.Any())
                {
                    return NotFound("No users found.");
                }

                // Return the list of UserIDs and Emails
                return Ok(users);
            }
            catch (Exception ex)
            {
                // Log the exception if you have a logging service (optional)
                // _logger.LogError(ex, "Error retrieving all user emails.");

                return StatusCode(500, "An error occurred while retrieving all user emails.");
            }
        }
        [HttpGet("borrowedbooksBetweenDates")]
        public async Task<IActionResult> GetBorrowedBooks([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var borrowedBooks = await _borrowedBookRepository.GetBorrowedBooksByDateRangeAsync(startDate, endDate);
            log.Info("Borrowed books Between a period fetched successfully");
            var result = borrowedBooks.Select(b => new
            {
                b.BorrowID,                           // Keep the original BorrowID
                UserName = b.User != null ? b.User.Name : "N/A",            // Get the UserName from the User navigation property
                BookTitle = b.Book != null ? b.Book.Title : "N/A",              // Get the Title from the Book navigation property
                b.BorrowDate,                         // Keep the original BorrowDate
                b.DueDate,                            // Keep the original DueDate
                b.ReturnDate,                         // Keep the original ReturnDate
                BorrowStatus = b.BorrowStatus.ToString() // Convert BorrowStatus enum to string if necessary
            });
            return Ok(result);
        }
    }
}