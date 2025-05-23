using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models.DTO
{
    public class OrderDto
    {
        [Required]
        public string ProductIdentifier { get; set; } = "";
        [Required,MinLength(30),MaxLength(150)]
        public string DeliveryAddress { get; set; } = "";
        [Required]
        public string PaymentMethod { get; set; } = "";
    }
}
