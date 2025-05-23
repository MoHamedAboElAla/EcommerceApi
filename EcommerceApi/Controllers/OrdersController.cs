using EcommerceApi.Models;
using EcommerceApi.Models.DTO;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {

        private readonly AppDbContext _context;
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create(OrderDto orderDto) 
        {
            //1- Check if the Payment Mehtod is valid or not 
            if (!OrderHelper.PaymentMethods.ContainsKey(orderDto.PaymentMethod))
            {
                ModelState.AddModelError("Payment Method", "Please select a valid Payment Method");
                return BadRequest(ModelState);
            }

            //2- Get the user id from the token
            var userId = JwtReader.GetUserId(User);
            var user = _context.Users.Find(userId); ;
            if (user == null)
            {
                ModelState.AddModelError("Order", "Unable to create an order");
                return BadRequest(ModelState);
            }
            var productDictionary = OrderHelper.GetProductDictionary(orderDto.ProductIdentifier);

            //3- Create the order
            Order order = new Order();
            order.UserId = userId;
            order.CreatedAt = DateTime.Now;
            order.PaymentMethod = orderDto.PaymentMethod;
            order.ShippingFee = OrderHelper.ShippingFee;
            order.DeliveryAddress = orderDto.DeliveryAddress;
            order.PaymentStatus = OrderHelper.PaymentStatus[0];
            order.OrderStatus = OrderHelper.OrderStatus[0];

            foreach (var pair in productDictionary)
            {
                int productId = pair.Key;
                var product = _context.Products.Find(productId);
                if (product == null)
                {
                    ModelState.AddModelError("Product", $"Product with id {productId} not found");
                    return BadRequest(ModelState);
                }
                var orderItem = new OrderItem();
                orderItem.ProductId = productId;
                orderItem.Quantity = pair.Value;
                orderItem.UnitPrice = product.Price;

                order.orderItems.Add(orderItem);
            }

            if(order.orderItems.Count < 1)
            {
                ModelState.AddModelError("Order", "No products found in the order");
                return BadRequest(ModelState);
            }

            //4- Save the order to the database
            _context.Orders.Add(order);
            _context.SaveChanges();

            //To Solve The problem of the object cycle
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.ReferenceHandler = ReferenceHandler.Preserve;
            options.WriteIndented = true;
            var OrderJson = JsonSerializer.Serialize(order, options);
            return Ok(OrderJson);

            
        }

    }
}
