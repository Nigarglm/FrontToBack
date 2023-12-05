using System.Text.RegularExpressions;
using _16Nov_task.Models;
using _16Nov_task.Utilities.Enums;
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
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityResult identityResult { get; private set; }

        public Accountcontroller(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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

                await _userManager.AddToRoleAsync(user,UserRole.Member.ToString());
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

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM,string? returnUrl)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }

            AppUser user = await _userManager.FindByNameAsync(loginVM.UsernameOrEmail);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(loginVM.UsernameOrEmail);
                if (user == null)
                {
                    ModelState.AddModelError(String.Empty,"Username, Email or Password is incorrect");
                    return View();
                }
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.IsRemembered,true);

            if(result.IsLockedOut)
            {
                ModelState.AddModelError(String.Empty, "You insert wrong password 3 times. Please try 30 seconds later.");
                return View();
            }

            if(!result.Succeeded)
            {
                ModelState.AddModelError(String.Empty, "Username, Email or Password is incorrect");
                return View();
            }

            if(returnUrl == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return Redirect(returnUrl);
        }

        public async Task<IActionResult> Logout()
               {
                      await _signInManager.SignOutAsync();
                      return RedirectToAction("Index", "Home");
               }

        public async Task<IActionResult> CreateRoles()
        {
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                if(!(await _roleManager.RoleExistsAsync(role.ToString())))
                {
                    await _roleManager.CreateAsync(new IdentityRole
                    {
                        Name = role.ToString()
                    });
                }
               
            }

            return RedirectToAction("Index", "Home");  
        }
        
    }
}
