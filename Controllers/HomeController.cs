using _16Nov_task.DAL;
using _16Nov_task.Models;
using _16Nov_task.Services;
using _16Nov_task.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _16Nov_task.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            List<Slide> slides = await _context.Slides.OrderBy(s => s.Order).Take(2).ToListAsync();
            List<Product> products = await _context.Products.Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary!=null)).OrderBy(p=>p.Id).Take(8).ToListAsync();

            {
                new Slide
                {

                    Title = "NEW PLANT",
                    Subtitle = "65% off",
                    Description = "Pronia,With 100% Natural, Organic & Plant Shop.",
                    Image = "slide1.jpeg",
                    Order = 2,
                };

                new Slide
                {

                    Title = "NEW PLANT",
                    Subtitle = "65% off",
                    Description = "Pronia,With 100% Natural, Organic & Plant Shop.",
                    Image = "slide2.jpeg",
                    Order = 3,
                };

                new Slide
                {
                    
                    Title = "NEW PLANT",
                    Subtitle="65% off",
                    Description="Pronia,With 100% Natural, Organic & Plant Shop.",
                    Image="slide3.jpeg",
                    Order=1,
                };
            };

            //_context.Slides.AddRange(slides);
            //_context.SaveChanges();

            HomeVM home = new HomeVM
            {
                Slides = slides,
                Products = products,
            };

            return View(home);
        }

        public IActionResult ErrorPage(string error="Xeta bash verdi")
        {
            return View(model:error);
        }
        public IActionResult About()
        {
            return View();
        }

        //public IActionResult Test()
        //{
        //    Response.Cookies.Append("Salam", new CookieOptions
        //    {
        //        MaxAge = TimeSpan.FromMinutes(30)
        //    }) ;

        //    HttpContext.Session.SetString("Salam2");

        //    return Ok();
        //}

        //public IActionResult GetCookie()
        //{
        //    string salam = Request.Cookies["Salam"];
        //    string salam2 = HttpContext.Session.GetString("Salam2");
        //    return Content(salam);
        //}


    }
}
