using _16Nov_task.Areas.ProniaAdmin.ViewModels;
using _16Nov_task.DAL;
using _16Nov_task.Models;
using _16Nov_task.Utilities.Extensions;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index(int page)
		{
            double count = await _context.Products.CountAsync();
            
			List<Product> products = await _context.Products.Skip(page*2).Take(2)
				.Include(p=>p.Category)
				.Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary==true))
				.Include(p=>p.ProductTags).ThenInclude(p=>p.Tag)
				.ToListAsync();

            PaginateVM<Product> paginateVM = new PaginateVM<Product>
            {
                CurrentPage = page + 1,
                TotalPage = Math.Ceiling(count / 2),
                Items = products
            };

           
            return View(paginateVM);
		}
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create()
		{
			ViewBag.Categories=await _context.Categories.ToListAsync();
			ViewBag.Tags=await _context.Tags.ToListAsync();
			return View();
		}
		[HttpPost]

		public async Task<IActionResult> Create(CreateProductVM productVM)
		{
            
            if (!ModelState.IsValid)
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

			if (!productVM.MainPhoto.ValidateType("image/"))
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

            if (!productVM.HoverPhoto.ValidateType("image/"))
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
				Url = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath,"assets","images","website-images")
			};
            ProductImage hoverImage = new ProductImage
            {
                Alt = productVM.Name,
                IsPrimary = false,
                Url = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
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
					Alt = productVM.Name,
					IsPrimary=null,
					Url=await photo.CreateFileAsync(_env.WebRootPath,"assets","images","website-images")
				});
			}

			await _context.Products.AddAsync(product);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
		{
			if (id <= 0) return BadRequest();

			Product existed = await _context.Products.Include(pi=>pi.ProductImages).Include(p=>p.ProductTags).FirstOrDefaultAsync(p => p.Id == id);
			

			if (existed == null) return NotFound();

			UpdateProductVM productVM = new UpdateProductVM()
			{
				Name = existed.Name,
				Price = existed.Price,
				SKU = existed.SKU,
				Description = existed.Description,
				CategoryId=(int)existed.CategoryId,
				TagIds=existed.ProductTags.Select(pt=>pt.TagId).ToList(),
				ProductImages= existed.ProductImages,
				Categories=await _context.Categories.ToListAsync(),
				Tags=await _context.Tags.ToListAsync()
			};

			return View(productVM);
		}
		[HttpPost]

		public async Task<IActionResult> Update(int id, UpdateProductVM productVM)
		{
            Product existed = await _context.Products.Include(pi=>pi.ProductImages).Include(p=>p.ProductTags).FirstOrDefaultAsync(p => p.Id == id);
			productVM.ProductImages = existed.ProductImages;
            if (!ModelState.IsValid)
			{
				productVM.Categories = await _context.Categories.ToListAsync();
				productVM.Tags= await _context.Tags.ToListAsync();
				return View(productVM);
			}

			
			if (existed == null) return NotFound();

			bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);

			if (!result)
			{
				productVM.Categories = await _context.Categories.ToListAsync();
                productVM.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("CategoryId", "Bele bir category movcud deyil");
				return View(productVM);
			}

			if(productVM.MainPhoto != null)
			{
				if (productVM.MainPhoto.ValidateType("image/"))
				{
					productVM.Categories=await _context.Categories.ToListAsync();
					productVM.Tags=await _context.Tags.ToListAsync();
					ModelState.AddModelError("MainPhoto", "File novu uygun deyil");
					return View(productVM);
				}
                if (productVM.MainPhoto.ValidateSize(600))
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await _context.Tags.ToListAsync();
                    ModelState.AddModelError("MainPhoto", "File olcusu uygun deyil");
                    return View(productVM);
                }
            }

            if (productVM.HoverPhoto != null)
            {
                if (productVM.HoverPhoto.ValidateType("image/"))
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await _context.Tags.ToListAsync();
                    ModelState.AddModelError("HoverPhoto", "File novu uygun deyil");
                    return View(productVM);
                }
                if (productVM.HoverPhoto.ValidateSize(600))
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await _context.Tags.ToListAsync();
                    ModelState.AddModelError("HoverPhoto", "File olcusu uygun deyil");
                    return View(productVM);
                }
            }

			

            existed.ProductTags.RemoveAll(pt=>!productVM.TagIds.Exists(tId=>tId==pt.TagId));


			List<int> creatable = productVM.TagIds.Where(tId => !existed.ProductTags.Exists(pt => pt.TagId == tId)).ToList();
			foreach (int tagId in creatable)
			{
				bool tagResult = await _context.Tags.AnyAsync(t=>t.Id==tagId);
				if (!tagResult)
				{
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await _context.Tags.ToListAsync();
                    ModelState.AddModelError("TagIds", "Bele bie tag movcud deyil");
                    return View(productVM);
                }
					existed.ProductTags.Add(new ProductTag
					{ TagId = tagId, });
			
			}

            if (productVM.MainPhoto != null)
            {
                string fileName = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage mainImage = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true);
                mainImage.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                _context.ProductImages.Remove(mainImage);
                existed.ProductImages.Add(new ProductImage
                {
                    Alt = productVM.Name,
                    IsPrimary = true,
                    Url = fileName
                });
            }

            if (productVM.HoverPhoto != null)
            {
                string fileName = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage hoverImage = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false);
                hoverImage.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                _context.ProductImages.Remove(hoverImage);
                existed.ProductImages.Add(new ProductImage
                {
                    Alt = productVM.Name,
                    IsPrimary = true,
                    Url = fileName
                });
            }

            foreach (ProductTag productTag in existed.ProductTags)
            {
                if (!productVM.TagIds.Exists(tId => tId == productTag.TagId))
                {
                    _context.ProductTags.Remove(productTag);
                }
            }

            if (productVM.ImageIds == null)
            {
                productVM.ImageIds = new List<int>();
            }

            List<ProductImage> removable = existed.ProductImages.Where(pi => !productVM.ImageIds.Exists(imgId => imgId == pi.Id) && pi.IsPrimary == null).ToList();
            foreach (ProductImage pImage in removable)
            {
                pImage.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(pImage);
            }

            TempData["Message"] = "";

			if (productVM.Photos != null)
			{
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

                    existed.ProductImages.Add(new ProductImage
                    {
                        Alt = productVM.Name,
                        IsPrimary = null,
                        Url = await photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
                    });
                }
            }

            existed.Name= productVM.Name;
			existed.Price= productVM.Price;
			existed.CategoryId=productVM.CategoryId;
			existed.SKU= productVM.SKU;
			existed.Description= productVM.Description;


			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));

		}

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.Include(p=>p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            foreach (ProductImage image in product.ProductImages)
            {
                image.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Detail()
        {
            List<Product> products = await _context.Products.Include(x=>x.ProductImages).ToListAsync();
            return View(products);
        }
    }
}
