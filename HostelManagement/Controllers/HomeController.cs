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
            // save the return URL
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            // if user is logged in, just redirect him
            if(HttpContext.User.Identity.IsAuthenticated && string.IsNullOrEmpty(returnUrl))
            {
                if (userManager.IsInRole(User.Identity.GetUserId(), "Admin"))
                {
                    return RedirectToAction("Index", "Home", new { area = "Administration" });
                }
                return RedirectToAction("Index", "User", new { area = "HostelMessManagement" });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(LoginViewModel model)
        {
            // Check if model is valid
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Find user in the database
            var user = await userManager.FindAsync(model.UserId, model.Password);

            // if user exists
            if (user != null)
            {
                // sign out any logged in users
                var ctx = Request.GetOwinContext();
                var authmanager = ctx.Authentication;
                authmanager.SignOut("ApplicationCookie");

                // sign in the user
                var identity = await userManager.CreateIdentityAsync(
                    user, DefaultAuthenticationTypes.ApplicationCookie);
                GetAuthenticationManager().SignIn(new AuthenticationProperties()
                {
                    AllowRefresh = true,
                    IsPersistent = false,
                    ExpiresUtc = DateTime.UtcNow.AddHours(1)
                }, identity);

                // redirect the user to the corresponding area
                if (string.IsNullOrEmpty(model.ReturnUrl))
                {
                    if (userManager.IsInRole(user.Id, "Admin"))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Administration" });
                    }
                    return RedirectToAction("Index", "User", new { area = "HostelMessManagement" });
                }
                else
                {
                    return Redirect(model.ReturnUrl);
                }
            }

            // add error if username is not valid
            ModelState.AddModelError("", "Invalid Username or password");
            return View(model);
        }

        public ActionResult MeetDevelopers()
        {
            return View();
        }

        private IAuthenticationManager GetAuthenticationManager()
        {
            var ctx = Request.GetOwinContext();
            return ctx.Authentication;
        }
    }
}