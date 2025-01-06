using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryManagementSystem.Models; // Add your models namespace
using System.Linq;
using LibraryManagementApi.Data;
using LibraryManagementApi.Dtos;

namespace LibraryManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly LibraryContext _con;

        public AuthController(IConfiguration configuration, LibraryContext conn)
        {
            _config = configuration;
            _con = conn;
        }

        // Validate method to check user credentials
        [NonAction]
        public User ValidateUser(string email, string password)
        {
            // Assuming you have a User model that stores email and password
            return _con.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Authenticate([FromBody] LoginRequest loginRequest)
        {
            IActionResult response = Unauthorized();

            // Validate the user
            var user = ValidateUser(loginRequest.Email, loginRequest.Password);
            if (user != null)
            {
                var userID = user.UserID;
                // Generate JWT token
                var token = GenerateJwtToken(user);
                return Ok(new
                {
                    UserId=userID,
                    Token=token
                });
            }

            // Return unauthorized if the user is not found or password is incorrect
            return response;
        }


        // Generate JWT token
        private string GenerateJwtToken(User user)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature);

            // Add user information and roles to the JWT token claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(12),  // Set the expiration time to 12 hours
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
