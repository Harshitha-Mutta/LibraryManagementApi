using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LibraryManagementApi.Models;
namespace LibraryManagementApi.Dtos
{
    public class ReservationUpdateDto
    {
        public int ReservationID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int BookID { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Status ReservationStatus { get; set; }
    }

}
