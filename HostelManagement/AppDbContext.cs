using Microsoft.AspNet.Identity.EntityFramework;

namespace HostelManagement
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext() : base("HostelManagement")
        {

        }
    }
}