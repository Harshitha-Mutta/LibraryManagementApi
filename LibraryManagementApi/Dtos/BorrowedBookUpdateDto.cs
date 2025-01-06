using LibraryManagementApi.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryManagementApi.Dtos
{
    public class BorrowedBookUpdateDto
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public int BookID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [JsonConverter(typeof(JsonDateOnlyConverter))]

        public DateTime ReturnDate { get; set; }

    }
}
