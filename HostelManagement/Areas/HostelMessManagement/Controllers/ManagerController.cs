using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HostelManagement.Areas.HostelMessManagement.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        // GET: HostelMessManagement/Manager
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