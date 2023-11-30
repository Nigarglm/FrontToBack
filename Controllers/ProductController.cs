using _16Nov_task.DAL;
using _16Nov_task.Models;
using _16Nov_task.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _16Nov_task.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public async Task<IActionResult> Details(int? id)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p=>p.ProductTags).ThenInclude(pt=>pt.Tag)
                .Include(p=>p.ProductColors).ThenInclude(pt=>pt.Color)
                .Include(p=>p.ProductSize).ThenInclude(pt=>pt.Size)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            List<Product> products = await _context.Products
                .Include(p=>p.ProductImages)
                .Where(p => p.CategoryId == product.CategoryId && p.Id!=product.Id)
                .ToListAsync();

            DetailVM detailVM = new DetailVM
            {
                Product = product,
                RelatedProducts = products
            };

            return View(detailVM);
        }

        
    }
}
