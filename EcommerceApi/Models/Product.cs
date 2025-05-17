using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]

        public string Brand { get; set; } = "";
        [MaxLength(100)]

        public string Category { get; set; } = "";
        [Precision(18, 2)]
        public decimal Price { get; set; }
        [MaxLength(500)]
        public string Description { get; set; } = "";
        [MaxLength(100)]
        public string ImageFileName { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
