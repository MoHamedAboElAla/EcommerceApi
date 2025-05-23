using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        [Precision(16, 2)]
        public decimal ShippingFee { get; set; }
        [MaxLength(100)]
        public string DeliveryAddress { get; set; } = "";
        [MaxLength(100)]
        public string PaymentMethod { get; set; } = "";
        [MaxLength(40)]
        public string PaymentStatus { get; set; } = "";
        [MaxLength(40)]
        public string OrderStatus { get; set; } = "";

        // Navigation properties
        public User User { get; set; } = null!;
        public List<OrderItem> orderItems { get; set; } = new ();

    }

}
