using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LibraryManagementApi.Helpers;

namespace LibraryManagementApi.Models
{
    public class Book
    {
        [Key]
        public int BookID { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public int GenreID { get; set; }
        [Required]
        public string PublisherName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [JsonConverter(typeof(JsonDateOnlyConverter))]

        public DateTime PublicationDate { get; set; }
        [Required]
        public string Language { get; set; }


        public string Description { get; set; }

        [Required]
        [Range(0,20)]
        public int AvailableCopies { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int TotalCopies { get; set; }

        //Navigation
        [ForeignKey(nameof(GenreID))]
        [JsonIgnore]
        public virtual Genre? Genre { get; set; }
        public virtual ICollection<BorrowedBook> BorrowedBooks { get; set; } = new List<BorrowedBook>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Fine> Fines { get; set; } = new List<Fine>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    }
}
