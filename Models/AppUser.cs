using _16Nov_task.Controllers;
using Microsoft.AspNetCore.Identity;

namespace _16Nov_task.Models
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Gender { get; internal set; }

        internal Task<IdentityResult> CreateAsync(AppUser user, string password)
        {
            throw new NotImplementedException();
        }

        //public static implicit operator AppUser(userManager<AppUser> userVM)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
