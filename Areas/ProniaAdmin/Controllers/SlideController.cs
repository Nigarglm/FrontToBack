using _16Nov_task.DAL;
using _16Nov_task.Models;
using _16Nov_task.Utilities.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;

namespace _16Nov_task.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

            bool result = _context.Categories.Any(s => s.Name.ToLower().Trim() == slide.Title.ToLower().Trim());
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
            if (!slide.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError("Photo", "Fayl tipi uygun deyil");
                return View();
            }
            if (!slide.Photo.ValidateSize(2*1024))
            {
                ModelState.AddModelError("Photo", "Faylin hecmi 2 mb-dan cox olmamalidir");
                return View();
            }

            
            slide.Image = await slide.Photo.CreateFile(_env.WebRootPath,"assests","images","website-images");



            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();

            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);

            if (existed == null) return NotFound();

            return View(existed);
        }
        [HttpPost]

        public async Task<IActionResult> Update(int id, Slide slide)
        {
            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existed == null) return NotFound();

            if (!ModelState.IsValid) return View();
            bool result = _context.Slides.Any(s => s.Title == slide.Title);

            if (result)
            {
                ModelState.AddModelError("Title", "Bu adli slide artiq movcuddur");
                return View();
            }

            if(slide.Photo != null)
            {
                if (!slide.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError("Photo", "Fayl tipi uygun deyil");
                    return View(existed);
                }

                if (!slide.Photo.ValidateSize(2 * 1024))
                {
                    ModelState.AddModelError("Photo", "Faylin hecmi 2 mb-dan boyuk olmamalidir");
                    return View(existed);
                }
                string newImage = await slide.Photo.CreateFile(_env.WebRootPath, "assests", "images", "website-images");
                existed.Image.DeleteFile(_env.WebRootPath, "assests", "images", "website-images");
                existed.Image = newImage;
            }

            existed.Title = slide.Title;
            existed.Photo = slide.Photo;
            existed.Subtitle = slide.Subtitle;
            existed.Order = slide.Order;
            existed.Description = slide.Description;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int id)
        {
            if(id<=0) return BadRequest();
            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide == null) return NotFound();

            slide.Image.DeleteFile(_env.WebRootPath, "assest", "images", "website-images");

            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
            
        }


        public async Task<IActionResult> Detail()
        {
            List<Slide> slides = await _context.Slides.ToListAsync();
            return View(slides);
        }
    }
}
