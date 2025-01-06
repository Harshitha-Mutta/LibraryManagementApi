using LibraryManagementApi.Data;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace LibraryManagementApi.Repository
{
    public class UserRepository : IUserRepository<User>
    {
        private readonly LibraryContext _context;
        private static readonly Dictionary<string, (int otp, DateTime genTime)> _otpStorage = new();

        public UserRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                // Log the exception (optional, based on your logging framework)
                // Example: _logger.LogError(ex, "Error fetching user by email: {Email}", email);

                // You might want to return null, or throw a specific exception, depending on your use case
                throw new Exception("An error occurred while fetching the user by email.", ex);
            }
        }


        public async Task<IEnumerable<User>> GetAllAdminsAsync()
        {
            return await _context.Users.Where(u => u.Role == RoleName.Admin).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.Where(u => u.Role == RoleName.User).ToListAsync();
        }

        public async Task AddAsync(User entity)
        {
            // Ensure BookCount is zero when adding new user
            entity.BookCount = 0;

            // Ensure the email is unique
            if (await GetUserByEmailAsync(entity.Email) != null)
            {
                throw new ArgumentException("Email already exists.");
            }

            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateBookCountAsync(int userId, int increment)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.BookCount = (user.BookCount ?? 0) + increment;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(User entity)
        {
            _context.Users.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            // Retrieve the user
            var user = await GetByIdAsync(id);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Check if the user has any active reservations
            var activeReservations = await _context.Reservations
                .Where(r => r.UserID == id && r.ReservationStatus == Status.Active) // Use 'ReservationStatus' and 'Status.Active'
                .ToListAsync();

            if (activeReservations.Any())
            {
                throw new Exception("User cannot be deleted due to active reservations.");
            }

            // Check if the user has any books borrowed (BookCount > 0)
            if (user.BookCount > 0)
            {
                throw new Exception("User cannot be deleted as they have borrowed books.");
            }

            // Check if the user has unpaid fines
            var unpaidFines = await _context.Fines
                .Where(f => f.UserID == id && f.PaidStatus == FineStatus.NotPaid) // Use 'PaidStatus' and 'FineStatus.NotPaid'
                .ToListAsync();

            if (unpaidFines.Any())
            {
                throw new Exception("User cannot be deleted due to unpaid fines.");
            }


            // If no issues, proceed with deletion
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUserCountAsync()
        {
            // Fetch the count of users from the database
            return await _context.Users.CountAsync();
        }

        public async Task SendOtpAsync(string email)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            Random random = new Random();
            int otp = random.Next(1000, 10000);

            string subject = "Reset Password OTP!";
            string body = $"Dear {user.Name},<br><br>The OTP to reset your password is {otp}";

            try
            {
                ReservationEmailNotificationService.SendEmail(user.Email, subject, body);
                _otpStorage[email] = (otp, DateTime.Now); // Store OTP and generation time
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while sending email: {ex.Message}");
            }
        }

        public bool VerifyOtp(string email, int otp)
        {
            if (_otpStorage.TryGetValue(email, out var otpData))
            {
                if (otpData.genTime.AddMinutes(10) > DateTime.Now && otpData.otp == otp)
                {
                    _otpStorage.Remove(email); // Optionally remove OTP after successful verification
                    return true;
                }
            }
            return false;
        }
        public async Task ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Here, hash the new password before saving it
            user.Password = (newPassword);
            await _context.SaveChangesAsync();
        }
        public async Task<User> GetUserByNameAsync(string name)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
        }


    }



}