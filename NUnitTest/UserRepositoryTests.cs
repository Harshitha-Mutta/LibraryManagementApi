using NUnit.Framework;
using LibraryManagementApi.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementApi.Repository;
using LibraryManagementApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace UnitTest
{
    public class UserRepositoryTests
    {
        private LibraryContext _context;
        private UserRepository _userRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: "LibraryManagementAPI") // In-memory DB setup
                .Options;

            _context = new LibraryContext(options);
            _userRepository = new UserRepository(_context);
        }

        [Test]
        public async Task AddUser_ShouldIncreaseUserCount()
        {
            // Arrange
            var testUser = new User
            {
                UserID = 111,
                Name = "John Doe",                    // Required: Name
                Email = "john.doe@example.com",       // Required: Email
                Password = "Password123!",            // Required: Password
                Address = "123 Main St",              // Required: Address
                PhoneNumber = "1234567890",           // Required: PhoneNumber (only digits)
                RegistrationDate = DateTime.Now,      // Required: RegistrationDate
                BookCount = 0,                        // Required: BookCount (within range 0-5)
                Role = RoleName.User                  // Required: Role
            };

            var initialCount = (await _userRepository.GetAllAsync()).Count();

            // Act
            await _userRepository.AddAsync(testUser);
            await _context.SaveChangesAsync();
            var countAfterAdding = (await _userRepository.GetAllAsync()).Count();

            // Assert
            Assert.AreEqual(initialCount + 1, countAfterAdding);
        }

        [Test]
        public async Task DeleteUser_ShouldDecreaseUserCount()
        {
            // Arrange
            var testUser = new User
            {
                UserID = 111,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Password = "Password123!",
                Address = "123 Main St",
                PhoneNumber = "1234567890",
                RegistrationDate = DateTime.Now,
                BookCount = 0,
                Role = RoleName.User
            };

            // Add the user to the repository
            await _userRepository.AddAsync(testUser);
            await _context.SaveChangesAsync();

            var initialCount = (await _userRepository.GetAllAsync()).Count();

            // Act
            await _userRepository.DeleteAsync(testUser.UserID);
            await _context.SaveChangesAsync();

            var countAfterDeleting = (await _userRepository.GetAllAsync()).Count();

            // Assert
            Assert.AreEqual(initialCount - 1, countAfterDeleting);
        }

        [Test]
        public async Task UpdateUser_ShouldModifyUserDetails()
        {
            // Arrange
            var testUser = new User
            {
                UserID = 111,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Password = "Password123!",
                Address = "123 Main St",
                PhoneNumber = "1234567890",
                RegistrationDate = DateTime.Now,
                BookCount = 0,
                Role = RoleName.User
            };

            // Add the user to the repository
            await _userRepository.AddAsync(testUser);
            await _context.SaveChangesAsync();

            // Act - Update User details
            testUser.Name = "John Smith"; // Update name
            testUser.Email = "john.smith@example.com"; // Update email
            await _userRepository.UpdateAsync(testUser);
            await _context.SaveChangesAsync();

            // Assert
            var updatedUser = await _userRepository.GetByIdAsync(testUser.UserID);
            Assert.AreEqual("John Smith", updatedUser.Name);
            Assert.AreEqual("john.smith@example.com", updatedUser.Email);
        }

        [Test]
        public async Task GetUserById_ShouldReturnCorrectUser()
        {
            // Arrange
            var testUser = new User
            {
                UserID = 111,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Password = "Password123!",
                Address = "123 Main St",
                PhoneNumber = "1234567890",
                RegistrationDate = DateTime.Now,
                BookCount = 0,
                Role = RoleName.User
            };

            // Add the user to the repository
            await _userRepository.AddAsync(testUser);
            await _context.SaveChangesAsync();

            // Act
            var retrievedUser = await _userRepository.GetByIdAsync(testUser.UserID);

            // Assert
            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual(testUser.UserID, retrievedUser.UserID);
        }

        [Test]
        public async Task GetAllAdmins_ShouldReturnOnlyAdmins()
        {
            // Arrange
            var adminUser = new User
            {
                UserID = 222,
                Name = "Admin User",
                Email = "admin@example.com",
                Password = "AdminPass123!",
                Address = "Admin St",
                PhoneNumber = "9876543210",
                RegistrationDate = DateTime.Now,
                BookCount = null, // Admins can have null BookCount
                Role = RoleName.Admin
            };

            var normalUser = new User
            {
                UserID = 333,
                Name = "Normal User",
                Email = "user@example.com",
                Password = "UserPass123!",
                Address = "User St",
                PhoneNumber = "0123456789",
                RegistrationDate = DateTime.Now,
                BookCount = 0,
                Role = RoleName.User
            };

            // Add users to the repository
            await _userRepository.AddAsync(adminUser);
            await _userRepository.AddAsync(normalUser);
            await _context.SaveChangesAsync();

            // Act
            var admins = await _userRepository.GetAllAdminsAsync();

            // Assert
            Assert.AreEqual(1, admins.Count());
            Assert.AreEqual(RoleName.Admin, admins.First().Role);
        }



        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}