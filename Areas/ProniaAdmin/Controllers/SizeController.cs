using _16Nov_task.DAL;
using _16Nov_task.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _16Nov_task.Areas.ProniaAdmin.Controllers
{

    [Area("ProniaAdmin")]
    public class SizeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public SizeController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            List<Size> sizes = await _context.Size.ToListAsync();
            return View(sizes);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Size size)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Categories.Any(s => s.Name.ToLower().Trim() == size.Name.ToLower().Trim());
            if (result)
            {
                ModelState.AddModelError("Name", "Bele bir category artiq movcuddur");
                return View();
            }

            await _context.Size.AddAsync(size);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();

            Size existed = await _context.Size.FirstOrDefaultAsync(s => s.Id == id);

            if (existed == null) return NotFound();

            return View(existed);
        }
        [HttpPost]

        public async Task<IActionResult> Update(int id, Size size)
        {
            Size existed = await _context.Size.FirstOrDefaultAsync(s => s.Id == id);
            if (existed == null) return NotFound();

            if (!ModelState.IsValid) return View();
            bool result = _context.Size.Any(t => t.Name == size.Name);

            if (result)
            {
                ModelState.AddModelError("Name", "Bu adli slide artiq movcuddur");
                return View();
            }

            existed.Name = size.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Size size = await _context.Size.FirstOrDefaultAsync(s => s.Id == id);
            if (size == null) return NotFound();



            _context.Size.Remove(size);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

    }
}
