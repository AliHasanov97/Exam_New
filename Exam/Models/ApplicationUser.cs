using Microsoft.AspNetCore.Identity;

namespace Exam.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Picture { get; set; }

        public bool IsActive { get; set; } = true;
    }


}
