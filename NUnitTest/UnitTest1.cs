using NUnit.Framework;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using LibraryManagementApi.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Models;
using LibraryManagementApi.Repository;
using LibraryManagementApi.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;


namespace NUnitTest
{
    
    public class Tests
    {
        private LibraryContext _context;
        private BookRepository _bookRepository;
        private GenreRepository _genreRepository;
        //private BorrowedBookRepository _borrowrepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: "LibraryManagementAPI") // In-memory DB setup
                .Options;

            // Initialize the context and repository using the in-memory database
            _context = new LibraryContext(options);
            _bookRepository = new BookRepository(_context);
            _genreRepository = new GenreRepository(_context);
            
        }

        [Test]
        public async Task AddBook_ShouldIncreaseBookCount()
        {
            // Arrange
            var testBook = new Book
            {
                BookID = 1200,
                Title = "Sample Book",
                Author = "John Doe",
                ISBN = "1234567890",
                GenreID = 1,
                PublisherName = "Sample Publisher",
                PublicationDate = new DateTime(2020, 1, 1),
                Language = "English",
                Description = "A sample book description.",
                AvailableCopies = 10,
                TotalCopies = 10
            };

            var initialCount = (await _bookRepository.GetAllAsync()).Count();

            // Act
            await _bookRepository.AddAsync(testBook);
            await _context.SaveChangesAsync(); // Ensure changes are saved to the in-memory database
            var countAfterAdding = (await _bookRepository.GetAllAsync()).Count();

            // Assert
            Assert.AreEqual(initialCount + 1, countAfterAdding);
        }
        [Test]
        public async Task DeleteBook_ShouldDecreaseBookCount()
        {
            // Arrange
            var testBook = new Book
            {
                BookID = 1200,
                Title = "Sample Book",
                Author = "John Doe",
                ISBN = "1234567890",
                GenreID = 1,
                PublisherName = "Sample Publisher",
                PublicationDate = new DateTime(2020, 1, 1),
                Language = "English",
                Description = "A sample book description.",
                AvailableCopies = 10,
                TotalCopies = 10
            };

            // Add the book to the repository
            await _bookRepository.AddAsync(testBook);
            await _context.SaveChangesAsync();

            var initialCount = (await _bookRepository.GetAllAsync()).Count();

            // Act
            await _bookRepository.DeleteAsync(testBook.BookID);
            await _context.SaveChangesAsync();

            var countAfterDeleting = (await _bookRepository.GetAllAsync()).Count();

            // Assert
            Assert.AreEqual(initialCount - 1, countAfterDeleting);
        }
        [Test]
        public async Task AddGenre_ShouldIncreaseGenreCount()
        {
            // Arrange
            var testGenre = new Genre
            {
                GenreID = 20,
                GenreName = "Science Fiction"
            };

            // Act
            var initialCount = (await _genreRepository.GetAllAsync()).Count();
            await _genreRepository.AddAsync(testGenre);
            await _context.SaveChangesAsync(); // Ensure changes are saved to the in-memory database
            var countAfterAdding = (await _genreRepository.GetAllAsync()).Count();

            // Assert
            Assert.AreEqual(initialCount + 1, countAfterAdding);
        }

        [Test]
        public async Task DeleteGenre_ShouldDecreaseGenreCount()
        {
            // Arrange
            var testGenre = new Genre
            {
                GenreID = 20,
                GenreName = "Science Fiction"
            };

            // Add the genre to the repository
            await _genreRepository.AddAsync(testGenre);
            await _context.SaveChangesAsync();

            var initialCount = (await _genreRepository.GetAllAsync()).Count();

            // Act
            await _genreRepository.DeleteAsync(testGenre.GenreID);
            await _context.SaveChangesAsync();

            var countAfterDeleting = (await _genreRepository.GetAllAsync()).Count();

            // Assert
            Assert.AreEqual(initialCount - 1, countAfterDeleting);
        }
        


        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

    }
}