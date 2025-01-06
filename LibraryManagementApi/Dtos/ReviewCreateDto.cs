using System.ComponentModel.DataAnnotations;

namespace LibraryManagementApi.Dtos
{
    public class ReviewCreateDto
    {
        [Required]
        public int BookID { get; set; } // Title of the book being reviewed

        [Required]
        public int UserID { get; set; } // Name of the user who wrote the review

        [Required]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } // Review description

        [Range(1, 5)]
        public int Rating { get; set; } // Rating from 1 to 5
    }
}
