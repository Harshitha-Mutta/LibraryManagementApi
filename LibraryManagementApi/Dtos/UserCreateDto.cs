using LibraryManagementApi.Helpers;
using LibraryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryManagementApi.Dtos
{
    public class UserCreateDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }  // Password field is required for creation

        [Required]
        public string Address { get; set; }

        [Required]
        [StringLength(10)]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain only digits.")]
        public string PhoneNumber { get; set; }



        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RoleName Role { get; set; }

    }
}