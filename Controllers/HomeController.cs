using _16Nov_task.DAL;
using _16Nov_task.Models;
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

        public IActionResult Index()
        {
            List<Slide> slides = _context.Slides.OrderBy(s => s.Order).Take(2).ToList();
            List<Product> products = _context.Products.Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary!=null)).OrderBy(p=>p.Id).Take(8).ToList();

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
        public IActionResult About()
        {
            return View();
        }
    }
}
