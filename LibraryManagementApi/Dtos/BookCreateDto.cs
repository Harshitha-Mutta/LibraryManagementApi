using LibraryManagementApi.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryManagementApi.Dtos
{
    public class BookCreateDto
    {
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

        public string Description { get; set; } // Optional field


        [Required]
        public int TotalCopies { get; set; }


    }
}
