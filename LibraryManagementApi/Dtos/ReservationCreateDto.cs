using LibraryManagementApi.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryManagementApi.Dtos
{
    public class ReservationCreateDto
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public string BookName { get; set; }


    }
}