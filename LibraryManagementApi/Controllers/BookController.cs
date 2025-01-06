using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementApi.Dtos;
using System.Threading.Tasks;
using log4net;
using LibraryManagementApi.Exceptions;

namespace LibraryManagementApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository<Book> _bookRepository;
        private static readonly ILog log = LogManager.GetLogger(typeof(BookController));

        public BookController(IBookRepository<Book> bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            log.Info("Fetching all books...");
            var books = await _bookRepository.GetAllAsync();
            if (books == null)
            {
                log.Warn("No books found");
                return NotFound();
            }
            log.Info("Books retrieved successfully");
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            log.Info($"Fetching book with ID: {id}");
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                log.Warn($"Book with ID {id} not found");
                return NotFound();
            }
            log.Info($"Book with ID {id} retrieved successfully");
            return Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook([FromBody] BookCreateDto bookDto)
        {
            log.Info("Adding a new book...");

            if (!ModelState.IsValid)
            {
                log.Warn("Invalid book data");
                return BadRequest(ModelState);
            }

            var book = new Book
            {
                Title = bookDto.Title,
                Author = bookDto.Author,
                ISBN = bookDto.ISBN,
                GenreID = bookDto.GenreID,
                PublisherName = bookDto.PublisherName,
                PublicationDate = bookDto.PublicationDate,
                Language = bookDto.Language,
                Description = bookDto.Description,
                AvailableCopies = bookDto.TotalCopies,
                TotalCopies = bookDto.TotalCopies
            };

            try
            {
                await _bookRepository.AddAsync(book);
                log.Info($"Book '{book.Title}' added successfully");
                return CreatedAtAction(nameof(GetBookById), new { id = book.BookID }, book);
            }
            catch (BookException ex)
            {
                log.Warn(ex.Message);
                return Conflict(new { message = ex.Message });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookCreateDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                log.Warn("Invalid model state for updating book");
                return BadRequest(ModelState);
            }

            try
            {
                var existingBook = await _bookRepository.GetByIdAsync(id);
                if (existingBook == null)
                {
                    log.Warn($"Book with ID {id} not found");
                    return NotFound($"Book with ID {id} not found.");
                }

                // Update the book entity using DTO
                existingBook.Title = bookDto.Title;
                existingBook.Author = bookDto.Author;
                existingBook.ISBN = bookDto.ISBN;
                existingBook.GenreID = bookDto.GenreID;
                existingBook.PublisherName = bookDto.PublisherName;
                existingBook.PublicationDate = bookDto.PublicationDate;
                existingBook.Language = bookDto.Language;
                existingBook.Description = bookDto.Description;
                existingBook.AvailableCopies = bookDto.TotalCopies; // Setting TotalCopies as AvailableCopies initially
                existingBook.TotalCopies = bookDto.TotalCopies;

                log.Info($"Updating book with ID: {id}");

                // Call UpdateAsync from the repository
                await _bookRepository.UpdateAsync(existingBook);

                log.Info($"Book with ID {id} updated successfully");
                return NoContent(); // HTTP 204: Successful update with no content
            }
            catch (BookException ex)
            {
                log.Error($"Error while updating book with ID {id}: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            log.Info($"Deleting book with ID: {id}");
            await _bookRepository.DeleteAsync(id);
            log.Info($"Book with ID {id} deleted successfully");
            return NoContent();
        }
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableBooks()
        {
            log.Info("Fetching available books...");
            var books = await _bookRepository.GetAvailableBooksAsync();
            if (books == null || !books.Any())
            {
                log.Warn("No available books found");
                return NotFound();
            }
            log.Info("Available books retrieved successfully");
            return Ok(books);
        }

        // GET: api/book/notavailable
        [HttpGet("notavailable")]
        public async Task<IActionResult> GetNotAvailableBooks()
        {
            log.Info("Fetching not available books...");
            var books = await _bookRepository.GetNotAvailableBooksAsync();
            if (books == null || !books.Any())
            {
                log.Warn("No available books found");
                return NotFound();
            }
            log.Info("Not available books retrieved successfully");
            return Ok(books);
        }
        
        [HttpGet("search")]
        public async Task<IActionResult> SearchBooks(
            [FromQuery] string? genreName,
            [FromQuery] string? author,
            [FromQuery] string? language,
            [FromQuery] string? publisherName,
            [FromQuery] string? title)
        {
            try
            {
                var books = await _bookRepository.SearchBooksAsync(genreName, author, language, publisherName, title);
                return Ok(books);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("title/{title}")]
        public async Task<IActionResult> GetBookByTitle(string title)
        {
            log.Info($"Fetching book with title: {title}");
            var book = await _bookRepository.GetBookByTitleAsync(title);
            if (book == null)
            {
                log.Warn($"Book with title {title} not found");
                return NotFound();
            }
            log.Info($"Book with title {title} fetched successfully");
            return Ok(book);
        }


        [HttpGet("total")]
        public async Task<IActionResult> GetTotalBooks()
        {
            try
            {
                var totalBooks = await _bookRepository.CalculateTotalBooksAsync();
                return Ok(new { totalBooks });
            }
            catch (BookException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpGet("search-suggestions")]
        public async Task<IActionResult> GetSearchSuggestions(string field, string query)
        {
            try
            {
                var suggestions = await _bookRepository.GetSearchSuggestionsAsync(field, query);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                throw new BookException("An error occurred while retrieving search suggestions.", ex);
            }
        }


    }
}
