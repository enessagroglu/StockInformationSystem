using Microsoft.AspNetCore.Mvc;
using StockInformationSystem.Infrastructure;
using StockInformationSystem.Core;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace StockInformationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ProductsController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/products
        [HttpPost]
        public IActionResult AddProduct(CreateProductDto productDto)
        {
            if (string.IsNullOrEmpty(productDto.Name) || productDto.Name.Any(char.IsDigit))
                return BadRequest("Product name cannot contain numbers.");

            if (productDto.StockQuantity <= 0 || productDto.Price <= 0)
                return BadRequest("Stock quantity and price must be greater than zero.");

            var product = new Product
            {
                Name = productDto.Name,
                StockQuantity = productDto.StockQuantity,
                Price = productDto.Price
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            return Ok(product);
        }


        // GET: api/products
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var products = _context.Products.ToList();
            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }


        // PUT: api/products/{id}/update-price
        [HttpPut("{id}/update-price")]
        public IActionResult UpdateProductPrice(int id, decimal newPrice)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound();

            if (newPrice <= 0)
                return BadRequest("Price must be greater than zero.");

            // Gecikme süresini `appsettings.json` dosyasından okuyalım
            var delayInMinutes = int.Parse(_configuration["PriceUpdateDelayInMinutes"]);

            // Gecikmeli işlemi farklı bir scope'da yürütmek için IServiceScopeFactory kullanacağız
            var serviceScopeFactory = HttpContext.RequestServices.GetService<IServiceScopeFactory>();

            Task.Delay(TimeSpan.FromMinutes(delayInMinutes)).ContinueWith(t =>
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var productToUpdate = scopedContext.Products.Find(id);
                    if (productToUpdate != null)
                    {
                        productToUpdate.Price = newPrice;
                        scopedContext.SaveChanges();
                    }
                }
            });

            return Ok($"Price update scheduled in {delayInMinutes} minutes.");
        }


        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();

            return NoContent();
        }

        // GET: api/products/filter
        [HttpGet("filter")]
        public IActionResult GetFilteredProducts(int? minStockQuantity, decimal? minPrice)
        {
            var products = _context.Products.AsQueryable();

            if (minStockQuantity.HasValue)
            {
                products = products.Where(p => p.StockQuantity >= minStockQuantity.Value);
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            return Ok(products.ToList());
        }
    }
}
