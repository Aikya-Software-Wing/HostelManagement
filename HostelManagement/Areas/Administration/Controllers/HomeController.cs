using HostelManagement.Areas.Administration.Models;
using Microsoft.AspNet.Identity;
using System.Web;
using System.Web.Mvc;

namespace HostelManagement.Areas.Administration.Controllers
{
    /// <summary>
    /// Home controller for admin area
    /// </summary>
    [Authorize(Roles = "Admin")]
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
        /// Action Method to display the index 
        /// </summary>
        /// <returns>Views</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Action Method to display the form to add a user
        /// </summary>
        /// <returns>View</returns>
        [HttpGet]
        public ActionResult AddUser()
        {
            return View();
        }

        /// <summary>
        /// Action Method to add user to the database
        /// </summary>
        /// <param name="model"> the user as specified by the admin</param>
        /// <returns>View</returns>
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> AddUser(AddUserViewModel model)
        {
            // if model is not valid, do not process anything
            if (!ModelState.IsValid)
            {
                return View();
            }

            // create the user
            var user = new AppUser()
            {
                UserName = model.Username
            };
            var result = await userManager.CreateAsync(user, model.Password);

            // if your was creates, add him or her to the required role
            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(user.Id, model.Role);
                return RedirectToAction("Index", "Home");
            }

            // if there were one ot more errors, add them to the model
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View();
        }

        /// <summary>
        /// Action Method to display the form the remove a user
        /// </summary>
        /// <returns>View</returns>
        public ActionResult RemoveUser()
        {
            return View();
        }

        /// <summary>
        /// Action Method to remove user by username
        /// </summary>
        /// <param name="username">username of user to be removed</param>
        /// <returns>string depicting error or success</returns>
        [HttpPost]
        public ActionResult RemoveUser(string username)
        {
            AppUser user = userManager.FindByName(username);
            if(user == null)
            {
                return Content("User not found!");
            }
            if (!username.Equals("admin"))
            {
                foreach (var role in user.Roles)
                {
                    userManager.RemoveFromRole(user.Id, role.RoleId);
                }
                userManager.Delete(user);
                return Content("User removed");
            }
            if(User.Identity.GetUserId().Equals(user.Id))
            {
                return Content("Can not remove current user");
            }
            return Content("Can not remove root account");
        }

        /// <summary>
        /// Action Method to logout the user
        /// </summary>
        /// <returns>View</returns>
        public ActionResult Logout()
        {
            // log out the user
            var ctx = Request.GetOwinContext();
            var authmanager = ctx.Authentication;
            authmanager.SignOut("ApplicationCookie");

            // redirect the user to login page
            return RedirectToAction("Index", "Home", new { area = ""});
        }

        /// <summary>
        /// Action method to display the meet developers page
        /// </summary>
        /// <returns>view</returns>
        public ActionResult MeetDevelopers()
        {
            return View();
        }
    }
}