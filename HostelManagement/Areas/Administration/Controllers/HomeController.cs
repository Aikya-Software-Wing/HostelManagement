using BusinessLayer;
using HostelManagement.Areas.Administration.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
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
        /// Action method to display the form to add a hostel
        /// </summary>
        /// <returns>a view</returns>
        public ActionResult AddHostel()
        {
            StudentHelper helper = new StudentHelper();
            ViewBag.genderList = new SelectList(helper.GetGenders(), "id", "val");

            return View();
        }

        /// <summary>
        /// Action method to add hostel
        /// </summary>
        /// <param name="userInput">the form filled by user</param>
        /// <returns>message in JSON format</returns>
        [HttpPost]
        public ActionResult AddHostel(AddHostelViewModel userInput)
        {
            StudentHelper helper = new StudentHelper();
            InfrastructureHelper helper1 = new InfrastructureHelper();

            ViewBag.genderList = new SelectList(helper.GetGenders(), "id", "val");

            return Json(helper1.AddHostel(userInput), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Action method to display a form to add a room
        /// </summary>
        /// <returns>a view</returns>
        [HttpGet]
        public ActionResult AddRooms()
        {
            InfrastructureHelper helper = new InfrastructureHelper();

            ViewBag.hostelBlocks = new SelectList(helper.GetHostelBlocks(), "blockNumber", "blockNumber");
            ViewBag.roomTypes = new SelectList(helper.GetRoomTypes(), "id", "val");

            return View();
        }

        /// <summary>
        /// Action method to add a room
        /// </summary>
        /// <param name="userInput">the form filled by the user</param>
        /// <returns>a view</returns>
        [HttpPost]
        public ActionResult AddRooms(Room userInput)
        {
            InfrastructureHelper helper = new InfrastructureHelper();

            ViewBag.hostelBlocks = new SelectList(helper.GetHostelBlocks(), "blockNumber", "blockNumber");
            ViewBag.roomTypes = new SelectList(helper.GetRoomTypes(), "id", "val");

            if(!ModelState.IsValid)
            {
                return View(userInput);
            }

            helper.AddRoom(userInput);

            return View();
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