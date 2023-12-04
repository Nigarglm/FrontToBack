using System.Text.RegularExpressions;
using _16Nov_task.Models;
using _16Nov_task.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace _16nov_task.controllers
{
    public class Accountcontroller : Controller
    {
        private object usermanager;
        private object result;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public IdentityResult identityResult { get; private set; }

        public Accountcontroller(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM userVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            AppUser user = new AppUser
            {
                Name = userVM.Name,
                Email = userVM.Email,
                Surname = userVM.Surname,
                UserName = userVM.Username,
                Gender = userVM.Gender
            };
            IdentityResult result = await _userManager.CreateAsync(user, userVM.Password);

            if (ModelState.IsValid)
            {
                if (IsValidEmail(userVM.Email)) { }
                else
                {
                    ModelState.AddModelError("Email", "Email adresi duzgun daxil edilmeyib");
                }

                userVM.Name = CapitalizeString(userVM.Name);
                userVM.Surname = CapitalizeString(userVM.Surname);
            }

                if (!result.Succeeded)
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("index", "home");
            
        }

                private bool IsValidEmail(string email)
                {
                        string emailPattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";
                        return Regex.IsMatch(email, emailPattern);
                }

               private string CapitalizeString(string input)
               {
                       if (string.IsNullOrEmpty(input))
                       {
                           return input;
                       }
                       return char.ToUpper(input[0]) + input.Substring(1).ToLower();
               }

               public async Task<IActionResult> Logout()
               {
                      await _signInManager.SignOutAsync();
                      return RedirectToAction("Index", "Home");
               }
        
    }
}
