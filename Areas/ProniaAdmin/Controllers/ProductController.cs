using _16Nov_task.Areas.ProniaAdmin.ViewModels;
using _16Nov_task.DAL;
using _16Nov_task.Models;
using _16Nov_task.Utilities.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _16Nov_task.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
	public class ProductController : Controller
	{
		private readonly AppDbContext _context;
		public ProductController(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			List<Product> products = await _context.Products
				.Include(p=>p.Category)
				.Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary==true)).ToListAsync();

			return View(products);
		}

		public async Task<IActionResult> Create()
		{
			ViewBag.Categories=await _context.Categories.ToListAsync();
			return View();
		}
		[HttpPost]

		public async Task<IActionResult> Create(CreateProductVM productVM)
		{
            
            if (ModelState.IsValid)
			{
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(productVM);
			}
			bool result = await _context.Products.AnyAsync(c=>c.Id==productVM.CategoryId);
			if(!result)
			{
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ModelState.AddModelError("CategoryId","Bu Id-li category movcud deyil");
				return View(productVM);
			}

			Product product = new Product
			{
				Name = productVM.Name,
				Price = productVM.Price,
				SKU = productVM.SKU,
				CategoryId = (int)productVM.CategoryId,
				Description = productVM.Description
			};

			await _context.Products.AddAsync(product);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Update(int id)
		{
			if (id <= 0) return BadRequest();

			Product existed = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

			if (existed == null) return NotFound();

			UpdateProductVM productVM = new UpdateProductVM()
			{
				Name = existed.Name,
				Price = existed.Price,
				SKU = existed.SKU,
				Description = existed.Description,
				CategoryId=(int)existed.CategoryId,
				Categories=await _context.Categories.ToListAsync(),
			};

			return View(productVM);
		}
		[HttpPost]

		public async Task<IActionResult> Update(int id, UpdateProductVM productVM)
		{

			if (!ModelState.IsValid)
			{
				productVM.Categories = await _context.Categories.ToListAsync();
				return View();
			}

			Product existed = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
			if (existed == null) return NotFound();

			bool result = await _context.Products.AnyAsync(p => p.Name == productVM.Name);

			if (!result)
			{
				productVM.Categories = await _context.Categories.ToListAsync();
				ModelState.AddModelError("Name", "Bu adli product artiq movcuddur");
				return View();
			}

			existed.Name= productVM.Name;
			existed.Price= productVM.Price;
			existed.CategoryId=productVM.CategoryId;
			existed.SKU= productVM.SKU;
			existed.Description= productVM.Description;


			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));

		}

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Detail()
        {
            List<Product> products = await _context.Products.ToListAsync();
            return View(products);
        }
    }
}
