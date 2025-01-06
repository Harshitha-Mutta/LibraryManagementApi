using LibraryManagementApi.Data;
using LibraryManagementApi.Exceptions;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagementApi.Repository
{
    public class BookRepository : IBookRepository<Book>
    {
        private readonly LibraryContext _context;

        public BookRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            try
            {
                return await _context.Books.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new BookException("An error occurred while retrieving all books.", ex);
            }
        }

        public async Task<Book> GetByIdAsync(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);

                if (book == null)
                {
                    throw new BookException($"Book with ID {id} not found.");
                }

                return book;
            }
            catch (Exception ex)
            {
                throw new BookException("An error occurred while retrieving the book.", ex);
            }
        }

        public async Task AddAsync(Book entity)
        {
            var existingBook = await _context.Books
                .AnyAsync(b => b.Title == entity.Title);

            if (existingBook)
            {
                throw new BookException("A book with this title already exists.");
            }

            try
            {
                await _context.Books.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new BookException("An error occurred while adding the book.", ex);
            }
        }

        public async Task UpdateAvailableCopiesAsync(int bookId, int Increment)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book != null)
            {
                book.AvailableCopies += Increment;
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateAsync(Book entity)
        {
            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new BookException("An error occurred while updating the book.", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    throw new BookException($"Book with ID {id} not found.");
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new BookException("An error occurred while deleting the book.", ex);
            }
        }
        public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
        {
            try
            {
                return await _context.Books
                    .Where(b => b.AvailableCopies > 0)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new BookException("An error occurred while retrieving available books.", ex);
            }
        }

        // Fetch books where AvailableCopies is equal to 0
        public async Task<IEnumerable<Book>> GetNotAvailableBooksAsync()
        {
            try
            {
                return await _context.Books
                    .Where(b => b.AvailableCopies == 0)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new BookException("An error occurred while retrieving not available books.", ex);
            }
        }
        public async Task<IEnumerable<Book>> SearchBooksAsync(
string genreName, string author, string language, string publisherName, string title)
        {
            try
            {
                var query = _context.Books.AsQueryable();

                // Filter by Genre if provided
                if (!string.IsNullOrEmpty(genreName))
                {
                    var lowerGenreName = genreName.ToLower();
                    query = query.Include(b => b.Genre)
                                 .Where(b => b.Genre != null && b.Genre.GenreName.ToLower() == lowerGenreName);
                }

                // Filter by Author if provided
                if (!string.IsNullOrEmpty(author))
                {
                    var lowerAuthor = author.ToLower();
                    query = query.Where(b => b.Author != null && b.Author.ToLower() == lowerAuthor);
                }

                // Filter by Language if provided
                if (!string.IsNullOrEmpty(language))
                {
                    var lowerLanguage = language.ToLower();
                    query = query.Where(b => b.Language != null && b.Language.ToLower() == lowerLanguage);
                }

                // Filter by Publisher if provided
                if (!string.IsNullOrEmpty(publisherName))
                {
                    var lowerPublisherName = publisherName.ToLower();
                    query = query.Where(b => b.PublisherName != null && b.PublisherName.ToLower() == lowerPublisherName);
                }

                // Filter by Title if provided
                if (!string.IsNullOrEmpty(title))
                {
                    var lowerTitle = title.ToLower();
                    query = query.Where(b => b.Title != null && b.Title.ToLower().Contains(lowerTitle));
                }

                // Return the list of books matching the criteria
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {

                throw new BookException("An error occurred while searching for books. ", ex);
            }
        }

        public async Task<int> CalculateTotalBooksAsync()
        {
            try
            {
                return await _context.Books.SumAsync(b => b.TotalCopies);
            }
            catch (Exception ex)
            {
                throw new BookException("An error occurred while calculating the total number of books.", ex);
            }
        }

        public async Task<Book> GetBookByTitleAsync(string title)
        {
            return await _context.Books.FirstOrDefaultAsync(b => b.Title == title);
        }
        public async Task<List<string>> GetSearchSuggestionsAsync(string field, string query)
        {
            var suggestions = new List<string>();

            if (!string.IsNullOrEmpty(query))
            {
                query = query.ToLower();

                switch (field.ToLower())
                {
                    case "title":
                        suggestions = await _context.Books
                            .Where(b => b.Title != null && b.Title.ToLower().Contains(query))
                            .Select(b => b.Title)
                            .Distinct()
                            .Take(10)  // Limit the number of suggestions
                            .ToListAsync();
                        break;

                    case "author":
                        suggestions = await _context.Books
                            .Where(b => b.Author != null && b.Author.ToLower().Contains(query))
                            .Select(b => b.Author)
                            .Distinct()
                            .Take(10)
                            .ToListAsync();
                        break;

                    case "genre":
                        suggestions = await _context.Genres
                            .Where(g => g.GenreName != null && g.GenreName.ToLower().Contains(query))
                            .Select(g => g.GenreName)
                            .Distinct()
                            .Take(10)
                            .ToListAsync();
                        break;

                    case "language":
                        suggestions = await _context.Books
                            .Where(b => b.Language != null && b.Language.ToLower().Contains(query))
                            .Select(b => b.Language)
                            .Distinct()
                            .Take(10)
                            .ToListAsync();
                        break;

                    case "publisher":
                        suggestions = await _context.Books
                            .Where(b => b.PublisherName != null && b.PublisherName.ToLower().Contains(query))
                            .Select(b => b.PublisherName)
                            .Distinct()
                            .Take(10)
                            .ToListAsync();
                        break;
                }
            }

            return suggestions;
        }



    }
}
