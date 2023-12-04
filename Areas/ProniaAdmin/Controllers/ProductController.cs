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
		private readonly IWebHostEnvironment _env;
		public ProductController(AppDbContext context,IWebHostEnvironment env)
		{
			_context = context;
			_env = env;
		}

		public async Task<IActionResult> Index()
		{
			List<Product> products = await _context.Products
				.Include(p=>p.Category)
				.Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary==true))
				.Include(p=>p.ProductTags).ThenInclude(p=>p.Tag)
				.ToListAsync();

			return View(products);
		}

		public async Task<IActionResult> Create()
		{
			ViewBag.Categories=await _context.Categories.ToListAsync();
			ViewBag.Tags=await _context.Tags.ToListAsync();
			return View();
		}
		[HttpPost]

		public async Task<IActionResult> Create(CreateProductVM productVM)
		{
            
            if (ModelState.IsValid)
			{
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                return View(productVM);
			}
			bool result = await _context.Products.AnyAsync(c=>c.Id==productVM.CategoryId);
			if(!result)
			{
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("CategoryId","Bu Id-li category movcud deyil");
				return View(productVM);
			}

			foreach (int tagId in productVM.TagIds)
			{
				bool tagResult = await _context.Tags.AnyAsync(t => t.Id == tagId);
				if (!tagResult)
				{
					ViewBag.Categories=await _context.Categories.ToListAsync();
                    ViewBag.Tags = await _context.Tags.ToListAsync();
					ModelState.AddModelError("TagIds", "Yalnis tag melumatlari gonderilib");
                    return View();
				}
			}

			if (productVM.MainPhoto.ValidateType("image/"))
			{
				ViewBag.Categories = await _context.Categories.ToListAsync();
				ViewBag.Tags = await _context.Tags.ToListAsync();
				ModelState.AddModelError("MainPhoto", "File tipi uygun deyil");
				return View();
			}
			if (!productVM.MainPhoto.ValidateSize(600))
			{
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("MainPhoto", "File olcusu uygun deyil");
                return View();
            }

            if (productVM.HoverPhoto.ValidateType("image/"))
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("HoverPhoto", "File tipi uygun deyil");
                return View();
            }
            if (!productVM.HoverPhoto.ValidateSize(600))
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("HoverPhoto", "File olcusu uygun deyil");
                return View();
            }

			ProductImage image = new ProductImage
			{
				Alt=productVM.Name,
				IsPrimary = true,
				Url = await productVM.MainPhoto.CreateFile(_env.WebRootPath,"assets","images","website-images")
			};
            ProductImage hoverImage = new ProductImage
            {
                Alt = productVM.Name,
                IsPrimary = false,
                Url = await productVM.HoverPhoto.CreateFile(_env.WebRootPath, "assets", "images", "website-images")
            };

            Product product = new Product
            {
                Name = productVM.Name,
                Price = productVM.Price,
                SKU = productVM.SKU,
                CategoryId = (int)productVM.CategoryId,
                Description = productVM.Description,
				ProductTags = new List<ProductTag>(),
				ProductImages=new List<ProductImage> {image,hoverImage}
            };

           
			foreach (int tagId in productVM.TagIds)
			{
				ProductTag productTag = new ProductTag
				{
					TagId = tagId,
				};

				product.ProductTags.Add(productTag);
			}

			TempData["Message"] = "";

			foreach (IFormFile photo in productVM.Photos)
			{
				if (!photo.ValidateType("image/"))
				{
					TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file tipi uygun deyil</p>";
					continue;
				}
				if (!photo.ValidateSize(600))
				{
                    TempData["Message"] += $"<p lass=\"text-danger\">{photo.FileName} file olcusu uygun deyil</p>";
                    continue;
				}

				product.ProductImages.Add(new ProductImage
				{
					Alt = product.Name,
					IsPrimary=null,
					Url=await photo.CreateFile(_env.WebRootPath,"assets","images","website-images")
				});
			}

			await _context.Products.AddAsync(product);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Update(int id)
		{
			if (id <= 0) return BadRequest();

			Product existed = await _context.Products.Include(p=>p.ProductTags).FirstOrDefaultAsync(p => p.Id == id);

			if (existed == null) return NotFound();

			UpdateProductVM productVM = new UpdateProductVM()
			{
				Name = existed.Name,
				Price = existed.Price,
				SKU = existed.SKU,
				Description = existed.Description,
				CategoryId=(int)existed.CategoryId,
				TagIds=existed.ProductTags.Select(pt=>pt.TagId).ToList(),
				Categories=await _context.Categories.ToListAsync(),
				Tags=await _context.Tags.ToListAsync()
			};

			return View(productVM);
		}
		[HttpPost]

		public async Task<IActionResult> Update(int id, UpdateProductVM productVM)
		{

			if (!ModelState.IsValid)
			{
				productVM.Categories = await _context.Categories.ToListAsync();
				productVM.Tags= await _context.Tags.ToListAsync();
				return View();
			}

			Product existed = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
			if (existed == null) return NotFound();

			bool result = await _context.Products.AnyAsync(p => p.Name == productVM.Name);

			if (!result)
			{
				productVM.Categories = await _context.Categories.ToListAsync();
                productVM.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("CategoryId", "Bele bir category movcud deyil");
				return View();
			}

			foreach (ProductTag productTag in existed.ProductTags)
			{
				if (!productVM.TagIds.Exists(tId => tId == productTag.TagId))
				{
					_context.ProductTags.Remove(productTag);
				}
			}

			List<ProductTag> removeable = existed.ProductTags.Where(pt=>!productVM.TagIds.Exists(tId=>tId==pt.TagId)).ToList();
			_context.ProductTags.RemoveRange(removeable);


            existed.ProductTags.RemoveAll(pt=>!productVM.TagIds.Exists(tId=>tId==pt.TagId));

			//foreach (int tagId in productVM.TagIds)
			//{
			//	if (!existed.ProductTags.Any(pt => pt.TagId == tagId))
			//	{
			//		existed.ProductTags.Add(new ProductTag
			//		{
			//			TagId = tagId
			//		});
			//	}
			//}

			List<int> creatable = productVM.TagIds.Where(tId => !existed.ProductTags.Exists(pt => pt.TagId == tId)).ToList();
			foreach (int tagId in creatable)
			{
				bool tagResult = await _context.Tags.AnyAsync(t=>t.Id==tagId);
				if (!tagResult)
				{
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await _context.Tags.ToListAsync();
                    ModelState.AddModelError("TagIds", "Bele bie tag movcud deyil");
                    return View();
                }
					existed.ProductTags.Add(new ProductTag
					{ TagId = tagId, });
			
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
