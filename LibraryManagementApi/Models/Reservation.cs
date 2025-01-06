using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LibraryManagementSystem.Models;
using System.Text.Json.Serialization;
using LibraryManagementApi.Helpers;

namespace LibraryManagementApi.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int BookID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [JsonConverter(typeof(JsonDateOnlyConverter))]

        public DateTime ReservationDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Status ReservationStatus { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(UserID))]
        [JsonIgnore]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(BookID))]
        [JsonIgnore]
        public virtual Book? Book { get; set; }
        [Required]
        public int QueuePosition { get; set; }
    }
    public enum Status
    {
        Active,
        Cancelled,
        Approved,
        Completed
    }
}
