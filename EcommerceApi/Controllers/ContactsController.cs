using EcommerceApi.Models;
using EcommerceApi.Models.DTO;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailSender _emailSender;

        public ContactsController(AppDbContext context, EmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }
        //Get All Users
        [Authorize(Roles ="admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contact>>> GetContacts(int? page)
        {
            if (page == null || page < 1)
            {
                page = 1;
            }
            int pageSize = 5;
            int totalPages = 0;
            decimal totalRecords = await _context.Contacts.CountAsync();
            totalPages = (int)Math.Ceiling(totalRecords / pageSize);

            int skip = (int)(page - 1) * pageSize;

           var contact= await _context.Contacts.Include(s => s.Subject)
                .OrderByDescending(c=>c.Id)
                .Skip(skip)
                .ToListAsync();

            var response = new
            {
                Contacts = contact,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
                
            };
            return Ok(response);

        }
        //Get User By Id
        //GET: api/Contacts/5
        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Contact>> GetContact(int id)
        {
            var contact = await _context.Contacts.Include(s=>s.Subject)
            .FirstOrDefaultAsync(c=>c.Id==id);
            if (contact == null)
            {
                return NotFound();
            }
            return contact;
        }
        //Get User By Subject
       
        [HttpGet("subject")]
        public IActionResult GetSubjects()
        {
            var subject = _context.Subjects.ToList();
            return Ok(subject);
        }

        //Create User
        [HttpPost]
        public async Task<ActionResult<Contact>> Create(ContactDto contactDto)
        {
            var subject = await _context.Subjects.FindAsync(contactDto.SubjectId);
            if (subject==null)
            {
                ModelState.AddModelError("Subject", "Please Select a valid Subject");
                return BadRequest(ModelState);
            }
            var contact = new Contact
            {
                FirstName = contactDto.FirstName,
                LastName = contactDto.LastName,
                Email = contactDto.Email,
                Phone = contactDto.Phone,
                Message = contactDto.Message,
                Subject = subject,
                CreatedAt = DateTime.Now
            };
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            //Send Confirmation Email
            string emailSubject = "Contact Confirmation";
            string userName = contact.FirstName + " " + contact.LastName;
            string emailMessage = $"Hello {userName},\n\nThank you for contacting us. We will get back to you shortly." +
                $"\n\nBest Regards,\nBest Store"+
                "Your Message:\n "+ contactDto.Message;
       
           _emailSender.SendEmail(emailSubject, contact.Email, userName, emailMessage).Wait();

            return Ok( contact);
        }
        //Update User
        [HttpPut("{id}")]
        public async Task<ActionResult<Contact>> Update(int id, ContactDto contactDto) 
        {
            var subject = await _context.Subjects.FindAsync(contactDto.SubjectId);
            if (subject == null)
            {
                ModelState.AddModelError("Subject", "Please Select a valid Subject");
                return BadRequest(ModelState);
            }
            var contact = _context.Contacts.Find(id);
            if (contact == null)
            {
                return NotFound();
            }
            contact.FirstName = contactDto.FirstName;
            contact.LastName = contactDto.LastName;
            contact.Email = contactDto.Email;
            contact.Phone = contactDto.Phone;
            contact.Message = contactDto.Message;
            contact.Subject = subject;
            await _context.SaveChangesAsync();
            return Ok(contact);

        }
        //Delete User
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Contact>> Delete(int id)
        {
            /*
               Method 2
         try{
            var contact = new Contact { Id = id };
            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            }
            catch(Exception e){
                return NotFound();
            }

            return Ok(contact);

            */
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return Ok(contact);
        }


    }
}
