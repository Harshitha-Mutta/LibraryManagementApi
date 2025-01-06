using System.ComponentModel.DataAnnotations;

public class UserUpdateDto
{
    public int UserID { get; set; }
    [Required]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string Address { get; set; }

    [Required]
    [StringLength(10)]
    [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain only digits.")]
    public string PhoneNumber { get; set; }

}
