using System.Text.RegularExpressions;
using _16Nov_task.Interfaces;
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
        private readonly IEmailService _emailService;

        public IdentityResult identityResult { get; private set; }

        public Accountcontroller(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager,IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
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

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(nameof(ConfirmEmail),"Account", new { token, Email=user.Email }, Request.Scheme);

            await _emailService.SendMailAsync(user.Email, "Email Confirmation", confirmationLink);
            //await _signInManager.SignInAsync(user, false);
            return RedirectToAction(nameof(SuccessfullyRegistered), "Account");
            
        }

        public async Task<IActionResult> ConfirmEmail(string token,string email)
        {
            AppUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (@result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            await _signInManager.SignInAsync(user, false);
            return View();
        }

        public IActionResult SuccessfullyRegistered()
        {
            return View();
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

            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(String.Empty, "Please confirm your email");
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
