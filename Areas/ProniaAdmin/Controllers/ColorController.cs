using _16Nov_task.Areas.ProniaAdmin.ViewModels;
using _16Nov_task.DAL;
using _16Nov_task.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _16Nov_task.Areas.ProniaAdmin.Controllers
{

    [Area("ProniaAdmin")]
    public class ColorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public ColorController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Colors.CountAsync();
            List<Color> colors = await _context.Colors.Skip(page * 2).Take(2).ToListAsync();

            PaginateVM<Color> paginateVM = new PaginateVM<Color>
            {
                CurrentPage = page + 1,
                TotalPage = Math.Ceiling(count / 2),
                Items = colors
            };

            return View(paginateVM);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Color color)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Colors.Any(c => c.Name.ToLower().Trim() == color.Name.ToLower().Trim());
            if (result)
            {
                ModelState.AddModelError("Name", "Bele bir reng artiq movcuddur");
                return View();
            }

            await _context.Colors.AddAsync(color);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();

            Color existed = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);

            if (existed == null) return NotFound();

            return View(existed);
        }
        [HttpPost]

        public async Task<IActionResult> Update(int id, Color color)
        {
            Color existed = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
            if (existed == null) return NotFound();

            if (!ModelState.IsValid) return View();
            bool result = _context.Colors.Any(c => c.Name == color.Name);

            if (result)
            {
                ModelState.AddModelError("Name", "Bu adli reng artiq movcuddur");
                return View();
            }

            existed.Name = color.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Color color = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
            if (color == null) return NotFound();



            _context.Colors.Remove(color);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

    }
}
