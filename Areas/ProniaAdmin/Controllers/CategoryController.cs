using System.Collections.Generic;
using _16Nov_task.Areas.ProniaAdmin.ViewModels;
using _16Nov_task.DAL;
using _16Nov_task.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _16Nov_task.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    [AutoValidateAntiforgeryToken]
    public class CategoryController : Controller
    {
        public readonly AppDbContext _context;
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Categories.CountAsync();

            List<Category> categories = await _context.Categories.Skip(page * 2).Take(2).Include(c=>c.Products).ToListAsync();

            PaginateVM<Category> paginateVM = new PaginateVM<Category>
            {
                CurrentPage = page + 1,
                TotalPage = Math.Ceiling(count / 2),
                Items = categories
            };

            return View(paginateVM);
        }

        [Authorize(Roles = "Admin,Moderator")]
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

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();

            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }
        [HttpPost]

        public async Task<IActionResult> Update(int id,  Category category)
        {
            if(!ModelState .IsValid) return View();

            Category existed = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (existed == null) return NotFound();

            bool result = _context.Categories.Any(c=>c.Name == category.Name && c.Id!=id);

            if (result)
            {
                ModelState.AddModelError("Name", "Bu adli category artiq movcuddur");
                return View();
            }

            existed.Name = category.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if(id<=0) return BadRequest();

            Category existed = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (existed == null) return NotFound();

            _context.Categories.Remove(existed);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Detail()
        {
            List<Category> categories = await _context.Categories.Include(c => c.Products).ToListAsync();

            return View(categories);
        }


    }
}
