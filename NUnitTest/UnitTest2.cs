using NUnit.Framework;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using LibraryManagementApi.Repository;
using LibraryManagementApi.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Models;

namespace NUnitTest
{
    public class BorrowedBookUnitTest
    {
        private LibraryContext _context;
        private BorrowedBookRepository _borrowedBookRepository;
        //private ReservationEmailNotificationService _emailService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: "LibraryManagementAPI") // In-memory DB setup
                .Options;

            _context = new LibraryContext(options);
           // _borrowedBookRepository = new BorrowedBookRepository(_context);

            // Seed data
            var user = new User
            {
                UserID = 111,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Password = "Password123",
                Address = "123 Test St",
                PhoneNumber = "1234567890",
                RegistrationDate = DateTime.Now,
                BookCount = 0,
                Role = RoleName.User
            };

            var book = new Book
            {
                BookID = 111,
                Title = "Test Book",
                Author = "Author Name",
                ISBN = "1234567890123",
                PublisherName = "Test Publisher",
                PublicationDate = DateTime.Now,
                Language = "English",
                Description = "A test book description.",
                AvailableCopies = 5,
                TotalCopies = 5
            };

            var borrowedBook = new BorrowedBook
            {
                BorrowID = 999,
                UserID = 111,
                BookID = 111,
                BorrowDate = DateTime.Now.AddDays(-10),
                DueDate = DateTime.Now.AddDays(-1),
                ReturnDate = null // Book has not been returned
            };

            _context.Users.Add(user);
            _context.Books.Add(book);
            _context.BorrowedBooks.Add(borrowedBook);
            _context.SaveChanges();
        }

        [Test]
        public async Task AddBorrowedBook_ShouldIncreaseBorrowedBookCount()
        {
            // Arrange
            var borrowedBook = new BorrowedBook
            {
                BorrowID = 1000,
                UserID = 111,
                BookID = 111,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(10),
                ReturnDate = null // Book has not been returned
            };

            var initialCount = (await _borrowedBookRepository.GetAllAsync()).Count();

            // Act
            await _borrowedBookRepository.AddAsync(borrowedBook);
            var countAfterAdding = (await _borrowedBookRepository.GetAllAsync()).Count();

            // Assert
            Assert.AreEqual(initialCount + 1, countAfterAdding);
        }



        [Test]
        public async Task DeleteBorrowedBook_ShouldDecreaseBorrowedBookCount()
        {
            // Arrange
            var borrowedBook = new BorrowedBook
            {
                BorrowID = 1002,
                UserID = 111,
                BookID = 111,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(10),
                ReturnDate = null // Book has not been returned
            };

            await _borrowedBookRepository.AddAsync(borrowedBook);
            var initialCount = (await _borrowedBookRepository.GetAllAsync()).Count();

            // Act
            await _borrowedBookRepository.DeleteAsync(borrowedBook.BorrowID);
            var countAfterDeleting = (await _borrowedBookRepository.GetAllAsync()).Count();

            // Assert
            Assert.AreEqual(initialCount - 1, countAfterDeleting);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllBorrowedBooks()
        {
            // Arrange
            await _borrowedBookRepository.AddAsync(new BorrowedBook { BorrowID = 1, BookID = 111, UserID = 111, BorrowDate = DateTime.Now });

            // Act
            var result = await _borrowedBookRepository.GetAllAsync();

            // Assert
            Assert.AreEqual(2, result.Count());
        }


        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}