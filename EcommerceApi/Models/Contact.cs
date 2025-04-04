using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models
{
    public class Contact
    {
        public int Id { get; set; }
        [MaxLength(50)]

        public string FirstName { get; set; }= "";
        [MaxLength(50)]
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        [MaxLength(150)]
        public string Message { get; set; } = "";
        public required Subject Subject { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
