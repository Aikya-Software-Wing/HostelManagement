using HostelManagement.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HostelManagement.Controllers
{

    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly UserManager<AppUser> userManager;

        public HomeController() : this(Startup.UserManagerFactory.Invoke())
        {

        }

        public HomeController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && userManager != null)
            {
                userManager.Dispose();
            }

            base.Dispose(disposing);
        }


        // GET: Home
        [HttpGet]
        public ActionResult Index(string returnUrl)
        {
            var model = new LoginModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await userManager.FindAsync(model.UserId, model.Password);

            if (user != null)
            {
                var identity = await userManager.CreateIdentityAsync(
                    user, DefaultAuthenticationTypes.ApplicationCookie);

                GetAuthenticationManager().SignIn(identity);

                if (userManager.IsInRole(user.Id, "Admin"))
                {
                    return RedirectToAction("Index", "Home", new { area = "Administration" });
                }
                else if(userManager.IsInRole(user.Id, "Manager"))
                {
                    return RedirectToAction("Index", "User", new { area = "HostelMessManagement" });
                }
                return RedirectToAction("Index", "User", new { area = "HostelMessManagement" });
            }


            ModelState.AddModelError("", "Invalid Username or password");
            return View();
        }

        private IAuthenticationManager GetAuthenticationManager()
        {
            var ctx = Request.GetOwinContext();
            return ctx.Authentication;
        }
    }
}