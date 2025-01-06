using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using LibraryManagementApi.Models;
using System.Text.Json.Serialization;
using LibraryManagementApi.Helpers;


namespace LibraryManagementSystem.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } // Unique constraint should be set at the database level

        [Required]
        public string Password { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [StringLength(10)]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain only digits.")]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [JsonConverter(typeof(JsonDateOnlyConverter))]
        public DateTime RegistrationDate { get; set; }

        [Required]
        [Range(0, 5, ErrorMessage = "Book count must be between 0 and 5.")]
        public int? BookCount { get; set; } // Nullable for Admins


        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RoleName Role { get; set; }

        // Navigation properties
        public ICollection<BorrowedBook> BorrowedBooks { get; set; } = new List<BorrowedBook>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Fine> Fines { get; set; } = new List<Fine>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

    }
    public enum RoleName
    {
        User,
        Admin

    }

}