using _16Nov_task.DAL;
using _16Nov_task.Models;
using Microsoft.AspNetCore.Mvc;

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
            List<Slide> slides = new List<Slide>
            {
                new Slide
                {
                   
                    Title = "NEW PLANT",
                    Subtitle="65% off",
                    Description="Pronia,With 100% Natural, Organic & Plant Shop.",
                    Image="slide1.jpeg",
                    Order=2,
                },

                new Slide
                {
                   
                    Title = "NEW PLANT",
                    Subtitle="65% off",
                    Description="Pronia,With 100% Natural, Organic & Plant Shop.",
                    Image="slide2.jpeg",
                    Order=3,
                },

                new Slide
                {
                    
                    Title = "NEW PLANT",
                    Subtitle="65% off",
                    Description="Pronia,With 100% Natural, Organic & Plant Shop.",
                    Image="slide3.jpeg",
                    Order=1,
                }
            };

            _context.Slides.AddRange(slides);
            _context.SaveChanges();

            return View(slides.OrderBy(s=>s.Order).Take(2).ToList());
        }
        public IActionResult About()
        {
            return View();
        }
    }
}
