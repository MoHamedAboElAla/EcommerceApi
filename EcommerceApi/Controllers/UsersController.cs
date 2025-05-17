using EcommerceApi.Models.DTO;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace EcommerceApi.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }
        // GET: api/users
        //Get All Users
        [HttpGet]
        public IActionResult GetUsers(int? page)
        {
            if(page == null || page < 1)
            {
                 page = 1;
            }

            int pageSize = 5;
            int totalPages = 0;

            decimal count = _context.Users.Count();
            totalPages = (int)Math.Ceiling(count / pageSize);

            var users = _context.Users.OrderByDescending(i => i.Id)
              .Skip((int)(page-1) * pageSize)
              .Take(pageSize)
              .ToList();

            List<UserProfileDto> userProfiles = new List<UserProfileDto>();

            foreach (var user in users)
            {
                var userProfileDto = new UserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };

                userProfiles.Add(userProfileDto);

            }
            return Ok(userProfiles);
        }


        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var userProfileDto = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return Ok(userProfileDto);
        }
        }
}
