using Microsoft.AspNetCore.Identity;

namespace Blog.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string Name { get; set; }

        [PersonalData]
        public string LastName { get; set; }
    }
}
