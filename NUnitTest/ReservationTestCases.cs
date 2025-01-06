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

namespace ReservationUnitTest
{
    public class ReservationRepositoryTests
    {
        private LibraryContext _context;
        private ReservationRepository _reservationRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: "LibraryManagementAPI") // In-memory DB setup
                .Options;

            _context = new LibraryContext(options);
            _reservationRepository = new ReservationRepository(_context);

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
                AvailableCopies = 0, // Set to 0 to allow reservations
                TotalCopies = 5
            };

            _context.Users.Add(user);
            _context.Books.Add(book);
            _context.SaveChanges();
        }

        [Test]
        public async Task AddReservation_ShouldIncreaseReservationCount()
        {
            // Arrange
            var reservation = new Reservation
            {
                ReservationID = 222,
                UserID = 111,
                BookID = 111,
                ReservationDate = DateTime.Now,
                ReservationStatus = Status.Active
            };

            var initialCount = (await _reservationRepository.GetAllAsync()).Count();

            // Act
            await _reservationRepository.AddAsync(reservation);
            var countAfterAdding = (await _reservationRepository.GetAllAsync()).Count();

            // Assert
            Assert.AreEqual(initialCount + 1, countAfterAdding);
        }

        [Test]
        public async Task DeleteReservation_ShouldDecreaseReservationCount()
        {
            // Arrange
            var reservation = new Reservation
            {
                ReservationID = 333,
                UserID = 111,
                BookID = 111,
                ReservationDate = DateTime.Now,
                ReservationStatus = Status.Active
            };

            await _reservationRepository.AddAsync(reservation);
            var initialCount = (await _reservationRepository.GetAllAsync()).Count();

            // Act
            await _reservationRepository.DeleteAsync(reservation.ReservationID);
            var countAfterDeleting = (await _reservationRepository.GetAllAsync()).Count();

            // Assert
            Assert.AreEqual(initialCount - 1, countAfterDeleting);
        }

        [Test]
        public async Task GetReservationById_ShouldReturnCorrectReservation()
        {
            // Arrange
            var reservation = new Reservation
            {
                ReservationID = 444,
                UserID = 111,
                BookID = 111,
                ReservationDate = DateTime.Now,
                ReservationStatus = Status.Active
            };

            await _reservationRepository.AddAsync(reservation);

            // Act
            var result = await _reservationRepository.GetByIdAsync(reservation.ReservationID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(reservation.ReservationID, result.ReservationID);
        }

        [Test]
        public async Task GetByUserId_ShouldReturnUserReservations()
        {
            // Arrange
            var reservation1 = new Reservation
            {
                ReservationID = 555,
                UserID = 111,
                BookID = 111,
                ReservationDate = DateTime.Now,
                ReservationStatus = Status.Active
            };

            var reservation2 = new Reservation
            {
                ReservationID = 556,
                UserID = 111,
                BookID = 111,
                ReservationDate = DateTime.Now,
                ReservationStatus = Status.Active
            };

            await _reservationRepository.AddAsync(reservation1);
            await _reservationRepository.AddAsync(reservation2);

            // Act
            var result = (await _reservationRepository.GetByUserIdAsync(111)).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task GetActiveReservations_ShouldReturnOnlyActiveReservations()
        {
            // Arrange
            var reservation1 = new Reservation
            {
                ReservationID = 777,
                UserID = 111,
                BookID = 111,
                ReservationDate = DateTime.Now,
                ReservationStatus = Status.Active
            };

            var reservation2 = new Reservation
            {
                ReservationID = 778,
                UserID = 111,
                BookID = 111,
                ReservationDate = DateTime.Now,
                ReservationStatus = Status.Cancelled
            };

            await _reservationRepository.AddAsync(reservation1);
            await _reservationRepository.AddAsync(reservation2);

            // Act
            var activeReservations = (await _reservationRepository.GetActiveReservationsAsync()).ToList();

            // Assert
            Assert.AreEqual(1, activeReservations.Count);
            Assert.AreEqual(Status.Active, activeReservations[0].ReservationStatus);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}