using HostelManagement.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HostelManagement.Controllers
{
    /// <summary>
    /// The home controller
    /// </summary>

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


        /// <summary>
        /// Action method to display the login page
        /// </summary>
        /// <param name="returnUrl">the redirection URL</param>
        /// <returns>view</returns>
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

        /// <summary>
        /// Action method to log in the user
        /// </summary>
        /// <param name="model">the form filled by the user</param>
        /// <returns>view</returns>
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

        /// <summary>
        /// Action Method to display the page to meet the developers
        /// </summary>
        /// <returns>view</returns>
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