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

namespace FineUnitTest
{
    public class FineRepositoryTests
    {
        private LibraryContext _context;
        private FineRepository _fineRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: "LibraryManagementAPI") // In-memory DB setup
                .Options;

            _context = new LibraryContext(options);
            _fineRepository = new FineRepository(_context);

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
                BookCount = 1,
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
                AvailableCopies = 0,
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
        public async Task AddFine_ShouldIncreaseFineCount()
        {
            // Arrange
            var fine = new Fine
            {
                FineID = 222,
                UserID = 111,
                BookID = 111,
                Amount = 100,
                FineDate = DateTime.Now,
                PaidStatus = FineStatus.NotPaid
            };

            var initialCount = (await _fineRepository.GetAllAsync()).Count();

            // Act
            await _fineRepository.AddAsync(fine);
            var countAfterAdding = (await _fineRepository.GetAllAsync()).Count();

            // Assert
            Assert.AreEqual(initialCount + 1, countAfterAdding);
        }

        [Test]
        public async Task DeleteFine_ShouldDecreaseFineCount()
        {
            // Arrange
            var fine = new Fine
            {
                FineID = 333,
                UserID = 111,
                BookID = 111,
                Amount = 100,
                FineDate = DateTime.Now,
                PaidStatus = FineStatus.NotPaid
            };

            await _fineRepository.AddAsync(fine);
            var initialCount = (await _fineRepository.GetAllAsync()).Count();

            // Act
            await _fineRepository.DeleteAsync(fine.FineID);
            var countAfterDeleting = (await _fineRepository.GetAllAsync()).Count();

            // Assert
            Assert.AreEqual(initialCount - 1, countAfterDeleting);
        }

        [Test]
        public async Task GetFineById_ShouldReturnCorrectFine()
        {
            // Arrange
            var fine = new Fine
            {
                FineID = 444,
                UserID = 111,
                BookID = 111,
                Amount = 100,
                FineDate = DateTime.Now,
                PaidStatus = FineStatus.NotPaid
            };

            await _fineRepository.AddAsync(fine);

            // Act
            var result = await _fineRepository.GetByIdAsync(fine.FineID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(fine.FineID, result.FineID);
        }

        [Test]
        public async Task GetByUserId_ShouldReturnUserFines()
        {
            // Arrange
            var fine1 = new Fine
            {
                FineID = 555,
                UserID = 111,
                BookID = 111,
                Amount = 100,
                FineDate = DateTime.Now,
                PaidStatus = FineStatus.NotPaid
            };

            var fine2 = new Fine
            {
                FineID = 556,
                UserID = 111,
                BookID = 111,
                Amount = 200,
                FineDate = DateTime.Now,
                PaidStatus = FineStatus.Paid
            };

            await _fineRepository.AddAsync(fine1);
            await _fineRepository.AddAsync(fine2);

            // Act
            var result = (await _fineRepository.GetByUserIdAsync(111)).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task GetUnpaidFines_ShouldReturnOnlyUnpaidFines()
        {
            // Arrange
            var fine1 = new Fine
            {
                FineID = 777,
                UserID = 111,
                BookID = 111,
                Amount = 100,
                FineDate = DateTime.Now,
                PaidStatus = FineStatus.NotPaid
            };

            var fine2 = new Fine
            {
                FineID = 778,
                UserID = 111,
                BookID = 111,
                Amount = 200,
                FineDate = DateTime.Now,
                PaidStatus = FineStatus.Paid
            };

            await _fineRepository.AddAsync(fine1);
            await _fineRepository.AddAsync(fine2);

            // Act
            var unpaidFines = (await _fineRepository.GetUnpaidFinesAsync()).ToList();

            // Assert
            Assert.AreEqual(1, unpaidFines.Count);
            Assert.AreEqual(FineStatus.NotPaid, unpaidFines[0].PaidStatus);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}