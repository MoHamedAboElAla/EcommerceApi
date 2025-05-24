using EcommerceApi.Models;
using EcommerceApi.Models.DTO;
using EcommerceApi.Services;
using EllipticCurve.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [HttpGet]
        public IActionResult GetOrders(int? page)
        {
            var userId = JwtReader.GetUserId(User);
            var role = _context.Users.Find(userId)?.Role;

            IQueryable<Order> query = _context.Orders.Include(u => u.User)
                .Include(o => o.orderItems)
                .ThenInclude(p => p.Product);

            if (role != "Admin")
            {
                query = query.Where(o => o.UserId == userId);
            }
            query = query.OrderByDescending(i => i.Id);

            if (page == null || page < 1)
            {
                page = 1;
            }
            int pageSize = 5; // Number of orders per page
            int totalPages = 0;

            decimal count = query.Count(); // Get the total number of orders
            totalPages = (int)Math.Ceiling(count / pageSize); // Calculate total pages


            query = query.Skip(((int)page - 1) * pageSize)
                .Take(pageSize);


            var orders = query.ToList();


            ////To Solve The problem of the object cycle
            //JsonSerializerOptions options = new JsonSerializerOptions();
            //options.ReferenceHandler = ReferenceHandler.Preserve;
            //options.WriteIndented = true;
            //// Serialize the orders to JSON
            //var ordersJson = JsonSerializer.Serialize(orders, options);
            //// Return the serialized JSON as the response

            foreach (var order in orders)
            {
                foreach (var item in order.orderItems)
                {
                    item.Order = null; // Prevent circular reference by setting Product to null
                }
                order.User.Password = "";
            }
            var response = new
            {
                Orders = orders,
                TotalPages = totalPages,
                Page = page,
                PageSize = pageSize

            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult GetOrderById(int? id)
        {
            int userId = JwtReader.GetUserId(User);
            var role = _context.Users.Find(userId)?.Role;

            Order? order = null;

            if (role == "Admin")
            {
                order = _context.Orders.Include(u => u.User)
                    .Include(o => o.orderItems)
                    .ThenInclude(p => p.Product)
                    .FirstOrDefault(i => i.Id == id);
            }
            else
            {
                order = _context.Orders.Include(u => u.User)
                    .Include(o => o.orderItems)
                    .ThenInclude(p => p.Product)
                    .FirstOrDefault(i => i.Id == id && i.UserId == userId);
            }
            if (order == null)
            {
                return NotFound($"Order with id {id} not found");
            }
            foreach (var item in order.orderItems)
            {
                item.Order = null; // Prevent circular reference by setting Product to null
            }
            order.User.Password = ""; // Hide the password from the response

            return Ok(order);
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

            if (order.orderItems.Count < 1)
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


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int? id)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound($"Order with id {id} not found");
            }
            _context.Orders.Remove(order);
            _context.SaveChanges();
            return Ok(new
            {
                Message = $"Order with id {id} deleted successfully"

            });
        } 

    }
}
