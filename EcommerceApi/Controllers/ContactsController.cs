using EcommerceApi.Models;
using EcommerceApi.Models.DTO;
using EcommerceApi.Services;
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
      
        public ContactsController(AppDbContext context)
        {
            _context = context;
        }
        //Get All Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contact>>> GetContacts()
        {
            return await _context.Contacts.Include(s => s.Subject).ToListAsync();
        }
        //Get User By Id
        //GET: api/Contacts/5
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
