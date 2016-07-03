using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HostelManagement
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext() : base("HostelManagement")
        {

        }
    }
}