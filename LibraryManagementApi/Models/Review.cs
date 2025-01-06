using LibraryManagementApi.Helpers;
using LibraryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryManagementApi.Models
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; } // Primary key for the review

        [Required]
        public int BookID { get; set; } // Foreign key for the Book being reviewed

        [Required]
        public int UserID { get; set; } // Foreign key for the User who wrote the review

        [Required]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } // Review description

        [Range(1, 5)]
        public int Rating { get; set; } // Rating from 1 to 5

        [Required]
        [DataType(DataType.Date)]
        [JsonConverter(typeof(JsonDateOnlyConverter))]
        public DateTime ReviewDate { get; set; } 

        // Navigation properties
        [ForeignKey(nameof(BookID))]
        [JsonIgnore]
        public virtual Book Book { get; set; }

        [ForeignKey(nameof(UserID))]
        [JsonIgnore]
        public virtual User User { get; set; }
    }
}
