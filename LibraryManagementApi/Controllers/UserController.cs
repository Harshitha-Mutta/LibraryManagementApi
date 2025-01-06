using LibraryManagementApi.Dtos;
using LibraryManagementApi.Interfaces;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using log4net;

namespace LibraryManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository<User> _userRepository;
        private static readonly ILog log = LogManager.GetLogger(typeof(UserController));

        public UserController(IUserRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            log.Info("Fetching all users...");
            var users = await _userRepository.GetAllAsync();
            log.Info("Users fetched successfully");
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            log.Info($"Fetching user with ID: {id}");
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                log.Warn($"User with ID {id} not found");
                return NotFound();
            }
            log.Info($"User with ID {id} fetched successfully");
            return Ok(user);
        }

        // New method: GetUserByEmail
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            log.Info($"Fetching user with email: {email}");
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                log.Warn($"User with email {email} not found");
                return NotFound();
            }
            log.Info($"User with email {email} fetched successfully");
            return Ok(user);
        }

        // New method: GetAllAdmins
        [HttpGet("admins")]
        public async Task<IActionResult> GetAllAdmins()
        {
            log.Info("Fetching all admins...");
            var admins = await _userRepository.GetAllAdminsAsync();
            log.Info("Admins fetched successfully");
            return Ok(admins);
        }

        // New method: GetAllUsers
        [HttpGet("users")]
        public async Task<IActionResult> GetAllNormalUsers()
        {
            log.Info("Fetching all regular users...");
            var users = await _userRepository.GetAllUsersAsync();
            log.Info("Regular users fetched successfully");
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userCreateDto)
        {
            log.Info("Creating a new user...");
            if (!ModelState.IsValid)
            {
                log.Warn("Invalid user data");
                return BadRequest(ModelState);
            }

            var existingUser = await _userRepository.GetUserByEmailAsync(userCreateDto.Email);
            if (existingUser != null)
            {
                log.Warn($"User with email {userCreateDto.Email} already exists");
                return Conflict("A user with this email already exists.");
            }

            var user = new User
            {
                Name = userCreateDto.Name,
                Email = userCreateDto.Email,
                Password = userCreateDto.Password,
                Address = userCreateDto.Address,
                PhoneNumber = userCreateDto.PhoneNumber,
                RegistrationDate = DateTime.Now,
                BookCount = 0,
                Role = userCreateDto.Role
            };

            await _userRepository.AddAsync(user);
            log.Info($"User with email {user.Email} created successfully");
            return CreatedAtAction(nameof(GetUserById), new { id = user.UserID }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userUpdateDto)
        {
            log.Info($"Updating user with ID: {id}");

            // Check if the ID in the URL matches the ID in the DTO
            // Assuming UserID is not part of the DTO as it is not included in the UserUpdateDto definition
            if (id != userUpdateDto.UserID) // Adjust this check if UserID is included in the DTO
            {
                log.Warn("User ID mismatch");
                return BadRequest();
            }

            // Retrieve the existing user from the repository
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
            {
                log.Warn($"User with ID {id} not found");
                return NotFound();
            }

            // Update only the properties that are allowed to change
            existingUser.Name = userUpdateDto.Name; // Update Name
            existingUser.Email = userUpdateDto.Email; // Update Email
            existingUser.Password = userUpdateDto.Password; // Update Password
            existingUser.Address = userUpdateDto.Address; // Update Address
            existingUser.PhoneNumber = userUpdateDto.PhoneNumber; // Update PhoneNumber


            await _userRepository.UpdateAsync(existingUser);
            log.Info($"User with ID {id} updated successfully");

            return NoContent();
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            log.Info($"Attempting to delete user with ID: {id}");

            try
            {
                // Call the repository method to delete the user
                await _userRepository.DeleteAsync(id);
                log.Info($"User with ID {id} deleted successfully");

                // Return a NoContent response indicating successful deletion
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception and return a BadRequest with the error message
                log.Error($"Error deleting user with ID {id}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetUserCount()
        {
            log.Info("Fetching the total number of users...");
            var userCount = await _userRepository.GetUserCountAsync();
            log.Info($"Total number of users: {userCount}");
            return Ok(userCount);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetUserByName(string name)
        {
            log.Info($"Fetching user with name: {name}");
            var user = await _userRepository.GetUserByNameAsync(name);
            if (user == null)
            {
                log.Warn($"User with name {name} not found");
                return NotFound();
            }
            log.Info($"User with name {name} fetched successfully");
            return Ok(user);
        }



        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp(string email)
        {
            try
            {
                await _userRepository.SendOtpAsync(email);
                return Ok("OTP sent to your email.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            bool isValid = _userRepository.VerifyOtp(verifyOtpDto.Email, verifyOtpDto.Otp);
            if (isValid)
            {
                return Ok("OTP verified successfully.");
            }
            return BadRequest("Invalid OTP or OTP expired.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                await _userRepository.ResetPasswordAsync(resetPasswordDto.Email, resetPasswordDto.NewPassword);
                return Ok("Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}