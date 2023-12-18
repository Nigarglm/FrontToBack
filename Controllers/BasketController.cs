using System.Security.Claims;
using _16Nov_task.DAL;
using _16Nov_task.Interfaces;
using _16Nov_task.Models;
using _16Nov_task.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.ContentModel;

namespace _16Nov_task.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        public BasketController(AppDbContext context, UserManager<AppUser> userManager,IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        List<BasketItemVM> basketVM = new List<BasketItemVM>();
        public async Task<IActionResult> Index()
        {

            if(User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users
                    .Include(u => u.BasketItems.Where(bi=>bi.OrderId==null))
                    .ThenInclude(bi => bi.Product)
                    .ThenInclude(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                foreach (BasketItem item in user.BasketItems)
                {
                    basketVM.Add(new BasketItemVM
                    {
                        Name=item.Product.Name,
                        Price=item.Product.Price,
                        Count=item.Count,
                        SubTotal=item.Count*item.Product.Price,
                        Image=item.Product.ProductImages.FirstOrDefault()?.Url
                    });
                }
            }
            else
            {
                if (Request.Cookies["Basket"] != null)
                {
                    List<BasketCookieItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                    foreach (var basketCookieItem in basket)
                    {
                        Product product = await _context.Products.Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(p => p.Id == basketCookieItem.Id);
                        if (product != null)
                        {
                            BasketItemVM basketItemVM = new BasketItemVM
                            {
                                Id = product.Id,
                                Name = product.Name,
                                Image = product.ProductImages.FirstOrDefault().Url,
                                Price = product.Price,
                                Count = basketCookieItem.Count,
                                SubTotal = product.Price * basketCookieItem.Count,

                            };
                        }
                    }
                }
            }

           
            return View(basketVM);
        }

        public async Task<IActionResult> AddBasket(int id, int productId)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            Product product = await _context.Products.FirstOrDefaultAsync(p=>p.Id== id);
            if(product == null)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated)
            {
                AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi=>bi.OrderId==null)).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (user == null)
                {
                    return NotFound();
                }
                BasketItem item = user.BasketItems.FirstOrDefault(b=>b.ProductId==id);
                if(item == null)
                {
                    item= new BasketItem
                    {
                        AppUserId = user.Id,
                        ProductId = product.Id,
                        Price = product.Price,
                        Count = 1,
                        OrderId=null
                    };
                    user.BasketItems.Add(item);
                }
                else
                {
                    item.Count++;
                   // _context.BasketItems.Update(item);
                }

                if (user.BasketItems == null)
                {
                    return NotFound();
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                List<BasketCookieItemVM> basket;

                if (Request.Cookies["Basket"] != null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                    BasketCookieItemVM item = basket.FirstOrDefault(b => b.Id == id);
                    if (item == null)
                    {
                        BasketCookieItemVM basketCookieItemVM = new BasketCookieItemVM
                        {
                            Id = id,
                            Count = 1
                        };
                        basket.Add(basketCookieItemVM);
                    }
                    else
                    {
                        item.Count++;
                    }
                }
                else
                {
                    basket = new List<BasketCookieItemVM>();
                }

                string json = JsonConvert.SerializeObject(basket);

                Response.Cookies.Append("Basket", json);
            }

            
            var Product = new BasketItem
            {
                ProductId = productId,
                Price = product.Price,
                Quantity = 1
            };

            var exist = basketVM.FirstOrDefault(p => p.ProductId == productId);

            if (exist != null)
            {
                exist.Quantity++;
            }
            else
            {
                basketVM.Add(exist);
            }

            return RedirectToAction(nameof(Index), "Home");
        }

        public ActionResult RemoveFromBasket(int productId)
        {
            var productToRemove = basketVM.FirstOrDefault(p => p.ProductId == productId);

            if (productToRemove != null)
            {
                basketVM.Remove(productToRemove);
            }

            return RedirectToAction("Index");
        }
        public ActionResult AdjustQuantity(int productId, int newQuantity)
        {
            var productToAdjust = basketVM.FirstOrDefault(p => p.ProductId == productId);

            if (productToAdjust != null)
            {
                productToAdjust.Quantity = newQuantity;
            }

            return RedirectToAction(nameof(Index), "Home");
        }
        
        public async Task<IActionResult> GetBasket()
        {
            return Content(Request.Cookies["Basket"]);
        }

        public async Task<IActionResult> CheckOut()
        {
            AppUser user = await _userManager.Users
                .Include(u => u.BasketItems.Where(bi => bi.OrderId == null))
                .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(u=>u.Id==User.FindFirstValue(ClaimTypes.NameIdentifier));

            OrderVM orderVM = new OrderVM
            {
                BasketItems = user.BasketItems
            };
            return View(orderVM);
        }
        [HttpPost]
        public async Task<IActionResult> CheckOut(OrderVM orderVM)
        {
            AppUser user = await _userManager.Users
                .Include(u => u.BasketItems.Where(bi => bi.OrderId == null))
                .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if(!ModelState.IsValid)
            {
                orderVM.BasketItems = user.BasketItems;
                return View(orderVM);
            }

            decimal total = 0;
            foreach (BasketItem item in user.BasketItems)
            {
                item.Price = item.Product.Price;
                total += item.Price;
            }

            Order order = new Order
            {
                Status = null,
                Adress=orderVM.Adress,
                PurchaseAt=DateTime.Now,
                AppUserId=user.Id,
                BasketItems=user.BasketItems,
                TotalPrice=total
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            string body = @"<table border=""1"">  <thead>  <tr>
                          <th>Name</th>
                          <th>Price</th>
                          <th>Count</th>
                          </tr>  </thead>  <tbody>";

            foreach (var item in order.BasketItems)
            {
                body += @$"<tr>
                           <td>{item.Product.Name}</td>
                           <td>{item.Product.Price}</td>
                           <td>{item.Count}</td>
                        </tr>";
            }

            body += @"</tbody>  </table>";

            await _emailService.SendMailAsync(user.Email,"Your Order",body,true);

            return RedirectToAction("Index","Home");
        }
    }
}
