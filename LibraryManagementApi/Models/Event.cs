using LibraryManagementApi.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryManagementApi.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [JsonConverter(typeof(JsonDateOnlyConverter))]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(100)]
        public string EventName { get; set; }

        [Required]
        [StringLength(250)]
        public string Description { get; set; }

        [StringLength(500)]
        public string Timing { get; set; }

    }
}
