using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LibraryManagementSystem.Models;
using System.Text.Json.Serialization;
using LibraryManagementApi.Helpers;

namespace LibraryManagementApi.Models
{
    public class BorrowedBook
    {
        [Key]
        public int BorrowID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int BookID { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [JsonConverter(typeof(JsonDateOnlyConverter))]

        public DateTime BorrowDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [JsonConverter(typeof(JsonDateOnlyConverter))]

        public DateTime DueDate { get; set; }
        [DataType(DataType.Date)]
        [JsonConverter(typeof(JsonDateOnlyConverter))]

        public DateTime? ReturnDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BorrowStatus? BorrowStatus { get; set; }

        //Navigation
        [ForeignKey(nameof(UserID))]
        [JsonIgnore]
        public virtual User? User { get; set; }


        [ForeignKey(nameof(BookID))]
        [JsonIgnore]
        public virtual Book? Book { get; set; }

        public virtual ICollection<Fine> Fines { get; set; } = new List<Fine>();
    }
    public enum BorrowStatus
    {
        BookingSuccess,   // Indicates that the pre-booking was successful
        Cancelled,           // Indicates that the borrowing or reservation was cancelled
        BookingAllocated  // Indicates that the pre-booking was allocated successfully
    }


}
