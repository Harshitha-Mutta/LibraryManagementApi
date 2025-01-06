using System.ComponentModel.DataAnnotations;

namespace LibraryManagementApi.Models
{
    public class Genre
    {
        [Key]
        public int GenreID { get; set; }

        [Required]
        [MaxLength(30)]
        public string GenreName { get; set; }

        // Navigation Property
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
