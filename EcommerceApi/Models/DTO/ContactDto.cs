using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models.DTO
{
    public class ContactDto
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; } = "";
        [Required,MaxLength(50)]
        public string LastName { get; set; } = "";
        [Required, MaxLength(100),EmailAddress]
        public string Email { get; set; } = "";
        [MaxLength(100)]
        public string Phone { get; set; } = "";
        [Required,MinLength(10),MaxLength(1000)]
        public string Message { get; set; } = "";
        public int SubjectId { get; set; } 
    }
}
