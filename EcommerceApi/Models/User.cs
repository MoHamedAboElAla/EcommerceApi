using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models
{
    [Index("Email",IsUnique =true)]
    public class User
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string FirstName { get; set; } = "";
        [MaxLength(100)]
        public string LastName { get; set; } = "";
        [MaxLength(100)]
        public string Email { get; set; } = "";
        [MaxLength(100)]
        public string Password { get; set; } = "";
        [MaxLength(11)]
        public string Phone { get; set; } = "";
        [MaxLength(100)]
        public string Address { get; set; } = "";
        [MaxLength(10)]
        public string Role { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
