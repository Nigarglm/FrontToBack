using System.Reflection;
using _16Nov_task.Areas.ProniaAdmin.ViewModels;
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
        public async Task<IActionResult> Create(CreateSlideVM slideVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Categories.Any(s => s.Name.ToLower().Trim() == slideVM.Title.ToLower().Trim());
            if (result)
            {
                ModelState.AddModelError("Name", "Bele bir category artiq movcuddur");
                return View();
            }

            if(slideVM.Photo == null)
            {
                ModelState.AddModelError("Photo", "Mutleq sekil daxil edilmelidir");
                return View();
            }
            if (!slideVM.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError("Photo", "Fayl tipi uygun deyil");
                return View();
            }
            if (!slideVM.Photo.ValidateSize(2*1024))
            {
                ModelState.AddModelError("Photo", "Faylin hecmi 2 mb-dan cox olmamalidir");
                return View();
            }

            
            string fileName = await slideVM.Photo.CreateFile(_env.WebRootPath,"assests","images","website-images");

            Slide slide = new Slide()
            {
                Image= fileName,
                Title= slideVM.Title,
                Order= slideVM.Order,
                Subtitle= slideVM.Subtitle,
                Description= slideVM.Description
            };

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();

            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);

            if (existed == null) return NotFound();

            UpdateSlideVM slideVM = new UpdateSlideVM()
            {
                Image = existed.Image,
                Title = existed.Title,
                Order = existed.Order,
                Subtitle = existed.Subtitle,
                Description = existed.Description
            };

            return View(slideVM);
        }
        [HttpPost]

        public async Task<IActionResult> Update(int id, UpdateSlideVM slideVM)
        {
           

            if (!ModelState.IsValid) return View(slideVM);

			Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
			if (existed == null) return NotFound();

			bool result = _context.Slides.Any(s => s.Title == slideVM.Title);

            if (result)
            {
                ModelState.AddModelError("Title", "Bu adli slide artiq movcuddur");
                return View();
            }

            if(slideVM.Photo != null)
            {
                if (!slideVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError("Photo", "Fayl tipi uygun deyil");
                    return View(existed);
                }

                if (!slideVM.Photo.ValidateSize(2 * 1024))
                {
                    ModelState.AddModelError("Photo", "Faylin hecmi 2 mb-dan boyuk olmamalidir");
                    return View(existed);
                }
                string newImage = await slideVM.Photo.CreateFile(_env.WebRootPath, "assests", "images", "website-images");
                existed.Image.DeleteFile(_env.WebRootPath, "assests", "images", "website-images");
                existed.Image = newImage;
            }

            existed.Title = slideVM.Title;
            existed.Photo = slideVM.Photo;
            existed.Subtitle = slideVM.Subtitle;
            existed.Order = slideVM.Order;
            existed.Description = slideVM.Description;

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
