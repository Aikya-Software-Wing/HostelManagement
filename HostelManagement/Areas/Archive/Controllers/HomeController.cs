using BusinessLayer;
using HostelManagement.Areas.HostelMessManagement.Models;
using System.Web;
using System.Web.Mvc;

namespace HostelManagement.Areas.Archive.Controllers
{
    /// <summary>
    /// Home controller for Archive
    /// </summary>
    [Authorize(Roles = "User,Manager")]
    public class HomeController : Controller
    {
        /// <summary>
        /// Action method to display the index
        /// </summary>
        /// <returns>view</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Action method to log out user
        /// </summary>
        /// <returns>view</returns>
        public ActionResult Logout()
        {
            // sign out the user
            var ctx = Request.GetOwinContext();
            var authmanager = ctx.Authentication;
            authmanager.SignOut("ApplicationCookie");

            // redirect the user to login
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        /// <summary>
        /// Action Method to display form to search for a student
        /// </summary>
        /// <returns>view</returns>
        public ActionResult ViewStudent()
        {
            return View();
        }

        /// <summary>
        /// Action method to display student details
        /// </summary>
        /// <param name="userInput">the form filled by user</param>
        /// <returns>partial view containing the student details or an error message</returns>
        [HttpPost]
        public ActionResult ViewStudent(StudentSearchViewModel userInput)
        {
            StudentHelper helper = new StudentHelper();
            string error = "";
            DisplayStudentViewModel viewModel = helper.GetStudentDetails(userInput.bid, out error, true);
            if (viewModel != null)
            {
                ViewBag.bid = userInput.bid;
                return PartialView("_ViewStudent", viewModel);
            }
            return Content(error);
        }

        /// <summary>
        /// Action method to display meet developers page
        /// </summary>
        /// <returns>view</returns>
        public ActionResult MeetDevelopers()
        {
            return View();
        }

        /// <summary>
        /// Action method to get the list of studented for auto complete
        /// </summary>
        /// <param name="term">the input given by the user</param>
        /// <returns>a list of the candidates in JSON format</returns>
        public ActionResult GetStudentList(string term)
        {
            StudentHelper helper = new StudentHelper();
            return Json(helper.GetStudentListForAutoComplete(term), JsonRequestBehavior.AllowGet);
        }
    }
}