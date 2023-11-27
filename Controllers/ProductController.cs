using _16Nov_task.DAL;
using _16Nov_task.Models;
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

        public IActionResult Details(int? id)
        {
            if (id <= 0) return BadRequest();

            Product product = _context.Products.Include(p=>p.Category).FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        
    }
}
