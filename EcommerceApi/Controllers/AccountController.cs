using EcommerceApi.Models;
using EcommerceApi.Models.DTO;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly EmailSender _emailSender;

        public AccountController(IConfiguration configuration, AppDbContext context, EmailSender emailSender)
        {

            _configuration = configuration;
            _context = context;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(_context.Users.ToList());
        }

        [Authorize]
        [HttpGet("GetTokenClaims")]
        public IActionResult GetTokenClaims()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                Dictionary<string, string> claims = new Dictionary<string, string>();
                foreach (Claim claim in identity.Claims)
                {
                    claims.Add(claim.Type, claim.Value);
                }
                return Ok(claims);
            }
            return Ok();
        }
        //[HttpGet("testToken")]

        //public IActionResult TestToken()
        //{
        //    var user = new User
        //    {
        //        Id = 1,
        //        Role = "Admin"
        //    };
        //    string jwttoken = CreateToken(user);
        //    var response = new
        //    {
        //        token = jwttoken,
        //        message = "Token generated successfully"
        //    };
        //    return Ok(response);
        //}

        [HttpPost("Register")]
        public IActionResult Register(UserDto userDto)
        {
            var emailCount = _context.Users.Count(u => u.Email == userDto.Email);
            if (emailCount > 0)
            {
                return BadRequest(new { message = "Email already exists" });
            }
            //encrypt password
            var passwordHasher = new PasswordHasher<User>();
            var encryptedPassword = passwordHasher.HashPassword(new User(), userDto.Password);

            //create user
            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Password = encryptedPassword,
                Phone = userDto.Phone,
                Address = userDto.Address,
                Role = "User",
                CreatedAt = DateTime.Now
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            var jwt = CreateToken(user);

            var userProfile = new UserProfileDto
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
            var response = new
            {
                token = jwt,
                user = userProfile,
                message = "User registered successfully"
            };
            return Ok(response);
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>() {

                new Claim("id",""+ user.Id),
                new Claim("role", user.Role),
            };
            var strKey = _configuration["JwtSettings:Key"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(strKey!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;

        }
        [HttpPost("Login")]
        public IActionResult Login(string eamil, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == eamil);

            if (user == null)
            {
                return BadRequest(new { Error = "User not found" });
            }
            //verify password
            var passwordHasher = new PasswordHasher<User>();

            var result = passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("Error", "Invalid email or password ");
                return BadRequest(ModelState);

            }
            var jwt = CreateToken(user);
            var userProfile = new UserProfileDto
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

            var response = new
            {
                token = jwt,
                user = userProfile,
                message = "User logged in successfully"
            };
            return Ok(response);
        }

        [HttpPost("ForgotPassword")]
        public IActionResult ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }
            //delete any old password reset request
            var oldPassword = _context.PasswordResets.FirstOrDefault(u => u.Email == email);

            if (oldPassword != null)
            {
                _context.Remove(oldPassword);

            }

            //create password reset token
            string token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();

            var passwordReset = new PasswordReset
            {
                Email = email,
                Token = token,
                CreatedAt = DateTime.Now
            };
            _context.PasswordResets.Add(passwordReset);
            _context.SaveChanges();

            //send the password reset token by email to the user
            string emailSubject = "Password Reset ";
            string userName = user.FirstName + " " + user.LastName;
            string emailMessage = $"Hello {userName},\n\n" +
              "We received a request to reset your password. " +
              $"please copy the following token and paste it in the password reset form:\n\n" +
              $"Token: {token}\n\n" +
              $"Thank you,\nEcommerceApi Team";
            _emailSender.SendEmail(emailSubject, email, userName, emailMessage).Wait();

            return Ok(new { message = "Password reset token sent to your email" });
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword(string token, string password)
        {
            //check if the token is valid
            var passwordReset = _context.PasswordResets.FirstOrDefault(u => u.Token == token);
            if (passwordReset == null)
            {
                return BadRequest(new { message = "Invalid token" });
            }
            //check if the token is expired
            var user = _context.Users.FirstOrDefault(u => u.Email == passwordReset.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Wrong or expired token" });
            }
            //encrypt password
            var passwordHasher = new PasswordHasher<User>();
            var encryptedPassword = passwordHasher.HashPassword(new User(), password);
            
            //update user password
            user.Password = encryptedPassword;

            //delete old token
            _context.Remove(passwordReset);
            _context.SaveChanges();

            return Ok(new { message = "Password reset successfully" });
        }
        [Authorize]
        [HttpGet("Profile")]
        public IActionResult GetProfile()
        {
            int id = JwtReader.GetUserId(User);

            //Get user from database
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return Unauthorized();
            }
            var userProfile = new UserProfileDto
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
            return Ok(userProfile);
        }

        [Authorize]
        [HttpPut("UpdateProfile")]
        public IActionResult UpdateProfile(UserProfileUpdateDto userProfileUpdateDto)
        {
            int id = JwtReader.GetUserId(User);


            //Get user from database
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return Unauthorized();
            }
            //update user profile
            user.FirstName = userProfileUpdateDto.FirstName;
            user.LastName = userProfileUpdateDto.LastName;
            user.Email = userProfileUpdateDto.Email;
            user.Phone = userProfileUpdateDto.Phone ?? "";
            user.Address = userProfileUpdateDto.Address;

            _context.SaveChanges();

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

        [Authorize]
        [HttpPut("UpdatePassword")]
        public IActionResult UpdatePassword([Required,MinLength(8),MaxLength(100)]string password)
        {
           int id = JwtReader.GetUserId(User);


            //Get user from db
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return Unauthorized();
            }
            //encrypt password
            var passwordHasher = new PasswordHasher<User>();
            var encryptedPassword = passwordHasher.HashPassword(new User(), password);

            //update user password
            user.Password = encryptedPassword;

            _context.SaveChanges();
          
            return Ok(user);
        }



    

    }
}