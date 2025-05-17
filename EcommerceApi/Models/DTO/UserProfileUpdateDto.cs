using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models.DTO
{
    public class UserProfileUpdateDto
    {
        [MaxLength(50)]
        public string FirstName { get; set; } = "";
        [MaxLength(50)]
        public string LastName { get; set; } = "";
        [MaxLength(100), EmailAddress]
        public string Email { get; set; } = "";

        [Required, MaxLength(11)]
        public string Phone { get; set; } = "";

        [Required, MaxLength(100), MinLength(8)]
        public string Address { get; set; } = "";
    }
}
