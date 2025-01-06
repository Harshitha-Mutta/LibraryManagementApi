using System.ComponentModel.DataAnnotations;

namespace LibraryManagementApi.Dtos
{
    public class FineUpdateDto
    {
        [Key]
        public int FineID { get; set; }

        [Required]
        public int UserID { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}
