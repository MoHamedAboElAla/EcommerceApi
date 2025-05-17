using EcommerceApi.Models.DTO;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController(AppDbContext Context) : ControllerBase
    {
        private readonly AppDbContext _context = Context;

        [HttpGet]
        public IActionResult GetCart(string productIdentifier)
        {
            CartDto cartDto = new CartDto();
            cartDto.CartItems = new List<CartItemDto>();
            cartDto.SubTotal = 0;
            cartDto.ShippingFee = OrderHelper.ShippingFee;
            cartDto.TotalPrice = 0;

            var productDictionary = OrderHelper.GetProductDictionary(productIdentifier);

            foreach (var product in productDictionary)
            {
                int productId = product.Key;
                var productItem = _context.Products.FirstOrDefault(p => p.Id == productId);
                if(productItem == null)
                {
                    continue;
                }
                var cartItem = new CartItemDto
                {
                    Product = productItem,
                    Quantity = product.Value,

                };
                cartDto.CartItems.Add(cartItem);
                cartDto.SubTotal += productItem.Price * product.Value;
                cartDto.TotalPrice += cartDto.SubTotal + OrderHelper.ShippingFee;


            }
            return Ok(cartDto);
        }
        
    }
}
