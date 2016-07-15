using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

[assembly: OwinStartup(typeof(HostelManagement.Startup))]

namespace HostelManagement
{
    public class Startup
    {
        public static Func<UserManager<AppUser>> UserManagerFactory { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new Microsoft.Owin.Security.Cookies.CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Home/Index")
            });

            UserManagerFactory = () =>
            {
                var userManager = new UserManager<AppUser>(new UserStore<AppUser>(new AppDbContext()));

                userManager.UserValidator = new UserValidator<AppUser>(userManager)
                {
                    AllowOnlyAlphanumericUserNames = false
                };

                AppDbContext context = new AppDbContext();

                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                if (!roleManager.RoleExists("Admin"))
                {
                    var role = new IdentityRole();
                    role.Name = "Admin";
                    roleManager.Create(role);

                    var user = new AppUser();
                    user.UserName = "admin";
                    user.Email = "lalitadithya1996@hotmail.com";

                    var result = userManager.Create(user, "password");
                    if (result.Succeeded)
                    {
                        userManager.AddToRole(user.Id, "Admin");
                    }
                }

                if (!roleManager.RoleExists("Manager"))
                {
                    var role = new IdentityRole();
                    role.Name = "Manager";
                    roleManager.Create(role);
                }

                if (!roleManager.RoleExists("User"))
                {
                    var role = new IdentityRole();
                    role.Name = "User";
                    roleManager.Create(role);
                }

                return userManager;
            };

        }
    }
}
