using System.ComponentModel.DataAnnotations;

namespace LibraryManagementApi.Dtos
{
    public class BorrowReservedBookRequestDto
    {
        [Required]
        public int UserID { get; set; }
        [Required]
        public int BookID { get; set; }
    }
}
