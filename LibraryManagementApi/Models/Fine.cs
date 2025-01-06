using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LibraryManagementSystem.Models;
using System.Text.Json.Serialization;
using LibraryManagementApi.Helpers;

namespace LibraryManagementApi.Models
{
    public class Fine
    {
        [Key]
        public int FineID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int BookID { get; set; }

        public decimal Amount { get; set; }

        [DataType(DataType.Date)]
        [JsonConverter(typeof(JsonDateOnlyConverter))]

        public DateTime FineDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FineStatus PaidStatus { get; set; }

        //Navigation
        [ForeignKey(nameof(UserID))]
        [JsonIgnore]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(BookID))]
        [JsonIgnore]
        public virtual Book? Book { get; set; }
    }
    public enum FineStatus
    {
       Paid,
       NotPaid
    }
}
