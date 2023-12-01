using _16Nov_task.DAL;
using _16Nov_task.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _16Nov_task.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        public SlideController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Slide> slides = await _context.Slides.ToListAsync();
            return View(slides);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Slide slide)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Categories.Any(c => c.Name.ToLower().Trim() == slide.Title.ToLower().Trim());
            if (result)
            {
                ModelState.AddModelError("Name", "Bele bir category artiq movcuddur");
                return View();
            }

            if(slide.Photo == null)
            {
                ModelState.AddModelError("Photo", "Mutleq sekil daxil edilmelidir");
                return View();
            }
            if (!slide.Photo.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Photo", "Fayl tipi uygun deyil");
                return View();
            }
            if (slide.Photo.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("Photo", "Faylin hecmi 2 mb-dan cox olmamalidir");
                return View();
            }

            FileStream file = new FileStream(@"C:\Users\Work\Desktop\16Nov(FrontToBack)\wwwroot\assets\images\website-images\" + slide.Photo.FileName, FileMode.Create);

            await slide.Photo.CopyToAsync(file);
            file.Close();
            slide.Image = slide.Photo.FileName;



            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
