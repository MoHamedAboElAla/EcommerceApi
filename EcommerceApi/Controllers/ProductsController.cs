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
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly List<string> categoryList = new List<string> { "Electronics", "Fashion", "Home", "Books", "Sports" };
        public ProductsController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        // GET: api/products
        //Get All Products
        [HttpGet]
        public IActionResult GetProducts(string? search,string? category,
            int? minPrice,int? maxPrice,string? sort,string? order, int? page)
        {
            IQueryable<Product> query =  _context.Products;
            //Search Functionality
            if (search !=null)
            {
                query = query.Where(p=>p.Name.Contains(search)|| p.Description.Contains(search));
            }
            //Category Filter
            if (category != null)
            {
                query = query.Where(p => p.Category == category);
            }
            //Price Filter
            if(minPrice != null)
            {
                query = query.Where(p => p.Price >=minPrice);
            }
            if (maxPrice != null)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            //Sort Functionality
            if (sort != null)
            {
                if (order == "desc")
                {
                    switch (sort)
                    {
                        case "name":
                            query = query.OrderByDescending(p => p.Name);
                            break;
                        case "price":
                            query = query.OrderByDescending(p => p.Price);
                            break;
                        case "createdAt":
                            query = query.OrderByDescending(p => p.CreatedAt);
                            break;
                            case "id":
                            query = query.OrderByDescending(p => p.Id);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (sort)
                    {
                        case "name":
                            query = query.OrderBy(p => p.Name);
                            break;
                        case "price":
                            query = query.OrderBy(p => p.Price);
                            break;
                        case "createdAt":
                            query = query.OrderBy(p => p.CreatedAt);
                            break;
                        case "id":
                            query = query.OrderBy(p => p.Id);
                            break;
                        default:
                            break;
                    }
                }
            }
            //Pagination
            if (page != null || page < 1) page = 1;
            int pageSize = 5;
            int totalPages = 0;

            decimal totalRecords = query.Count();
            totalPages= (int)Math.Ceiling(totalRecords / pageSize);
            query = query.Skip((int)((page - 1) * pageSize)).Take(pageSize);


            var products =query.ToList();

            var response = new {
                Products = products,
                TotalPages=totalPages,
                PageSize=pageSize,
                Page=page
            };

            return Ok(response);
        }
        // GET: api/products/5
        //Get Product By Id
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        //Get Categories
        [HttpGet("categories")]
        public ActionResult<List<string>> GetCategories()
        {
            return Ok(categoryList);
        }
        //Create Product
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> Create([FromForm]ProductDto productDto)
        {
            //Check if the category is valid
            if (!categoryList.Contains(productDto.Category))
            {
                ModelState.AddModelError("Category", "Invalid category");
                return BadRequest(ModelState);
            }
            if (productDto.ImageFileName == null)
            {
                ModelState.AddModelError("ImageFileName", "Image is required");
                return BadRequest(ModelState);
            }
            //Save Image on the Server
            string imageFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            imageFileName +=Path.GetExtension(productDto.ImageFileName.FileName);

            string imagesFolder=_env.WebRootPath + "/images/products";
            using (var stream = System.IO.File.Create(imagesFolder + imageFileName)) { 
                            productDto.ImageFileName.CopyTo(stream);
            }
            var product =new Product()
            {
                Name = productDto.Name,
                Description = productDto.Description??"",
                Price = productDto.Price,
                Brand = productDto.Brand,
                Category = productDto.Category,
                ImageFileName = imageFileName,
                CreatedAt = DateTime.Now,
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Ok(product);
        }

        //Update Product
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> Update(int id, [FromForm] ProductDto productDto) 
        {
            //Check if the category is valid
            if (!categoryList.Contains(productDto.Category))
            {
                ModelState.AddModelError("Category", "Invalid category");
                return BadRequest(ModelState);
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            string imageFileName = product.ImageFileName;
            if (productDto.ImageFileName != null)
            {
                //Save Image on the Server
                imageFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                imageFileName += Path.GetExtension(productDto.ImageFileName.FileName);

                string imagesFolder = _env.WebRootPath + "/images/products";
                using (var stream = System.IO.File.Create(imagesFolder + imageFileName))
                {
                    productDto.ImageFileName.CopyTo(stream);
                }
                //Delete Old Image
                System.IO.File.Delete(imagesFolder + product.ImageFileName);
            }
            //Update Product
            product.Name = productDto.Name;
            product.Description = productDto.Description ?? "";
            product.Price = productDto.Price;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.ImageFileName = imageFileName;
            product.CreatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok(product);
        }
        [Authorize(Roles = "admin")]

        //Delete Product
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            //Delete Image
            string imagesFolder = _env.WebRootPath + "/images/products";
            System.IO.File.Delete(imagesFolder + product.ImageFileName);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(product);
        }

    }
}
