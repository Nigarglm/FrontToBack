using System.Collections.Generic;
using _16Nov_task.DAL;
using _16Nov_task.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _16Nov_task.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class CategoryController : Controller
    {
        public readonly AppDbContext _context;
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _context.Categories.Include(c=>c.Products).ToListAsync();

            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Categories.Any(c=>c.Name.ToLower().Trim()==category.Name.ToLower().Trim());
            if(result)
            {
                ModelState.AddModelError("Name", "Bele bir category artiq movcuddur");
                return View();
            }
            
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
