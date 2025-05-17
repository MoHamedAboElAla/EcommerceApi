using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models.DTO
{
    public class UserDto
    {
        public string FirstName { get; set; } = "";
        [MaxLength(100)]
        public string LastName { get; set; } = "";
        [MaxLength(100),EmailAddress]
        public string Email { get; set; } = "";
        [MaxLength(100)]
        public string Password { get; set; } = "";
        [Required,MaxLength(11)]
        public string Phone { get; set; } = "";
        [Required,MaxLength(100),MinLength(8)]
        public string Address { get; set; } = "";
    }
}
