using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HostelManagement.Areas.HostelMessManagement.Controllers
{
    [Authorize(Roles ="User")]
    public class UserController : Controller
    {
        // GET: HostelMessManagement/User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Logout()
        {
            var ctx = Request.GetOwinContext();
            var authmanager = ctx.Authentication;
            authmanager.SignOut("ApplicationCookie");
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}