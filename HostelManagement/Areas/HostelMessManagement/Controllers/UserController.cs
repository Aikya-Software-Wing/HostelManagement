using HostelManagement.Areas.HostelMessManagement.Models;
using HostelManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HostelManagement.Areas.HostelMessManagement.Controllers
{
    [Authorize(Roles = "User,Manager")]
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

        [HttpGet]
        public ActionResult AddStudent()
        {
            SetUpDropDownViewData();
            return View();
        }

        [HttpPost]
        public ActionResult AddStudent(StudentViewModel userInput)
        {
            if (!ModelState.IsValid)
            {
                SetUpDropDownViewData();
                return View(userInput);
            }
            if (userInput.semester > 1 && string.IsNullOrEmpty(userInput.usn))
            {
                ModelState.AddModelError("usn", "The USN is required");
                SetUpDropDownViewData();
                return View(userInput);
            }
            string bid = GenerateBid(userInput.branch, userInput.year);
            HostelManagementEntities db = new HostelManagementEntities();
            Student newStudent = new Student()
            {
                bid = bid,
                name = userInput.name.Trim(),
                usn = string.IsNullOrEmpty(userInput.usn) ? "" : userInput.usn,
                semester = userInput.semester,
                gender = userInput.gender,
                course = userInput.course,
                branch = userInput.branch,
                year = userInput.year,
                dob = userInput.dob
            };
            newStudent.Allotments.Add(new Allotment()
            {
                bid = bid,
                dateOfJoin = userInput.doj,
                hostelBlock = userInput.blockNumber,
                roomNum = userInput.roomNumber,
                year = userInput.year
            });
            db.Students.Add(newStudent);
            db.SaveChanges();
            db.Dispose();
            TempData["bid"] = bid;
            return RedirectToAction("AddStudentSuccess");
        }

        [HttpGet]
        public ActionResult AddStudentSuccess()
        {
            ViewBag.bid = TempData["bid"];
            TempData.Remove("bid");
            return View();
        }

        [NonAction]
        private string GenerateBid(int department, int year)
        {
            HostelManagementEntities db = new HostelManagementEntities();
            int numberOfStudent = db.Students.Where(x => x.branch == department).ToList().Count + 1;
            return year.ToString().Trim().Substring(2, 2) + db.Departments.Where(x => x.id == department).First().val + numberOfStudent.ToString();
        }

        public JsonResult GetFeeBreakUp(string gender, string roomType)
        {
            int gen = gender.Equals("Male") ? 1 : 2;
            int typ = roomType.Equals("attached") ? 1 : 2;
            HostelManagementEntities db = new HostelManagementEntities();
            decimal dailymess = db.HostelCharges.Where(x => x.id == 0).First().val.Value;
            int rentCode = int.Parse(gen+""+typ+"1");
            int fixedCode = int.Parse(gen + "" + typ + "2");
            int depCode = int.Parse(gen + "" + typ + "3");
            decimal rent = db.HostelCharges.Where(x => x.id == rentCode).First().val.Value;
            decimal fix = db.HostelCharges.Where(x => x.id == fixedCode).First().val.Value;
            decimal deposit = db.HostelCharges.Where(x => x.id == depCode).First().val.Value;
            List<decimal> charges = new List<decimal>();
            charges.Add(dailymess);
            charges.Add(rent);
            charges.Add(fix);
            charges.Add(deposit);
            return Json(charges, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBlocks(string gender)
        {
            HostelManagementEntities db = new HostelManagementEntities();
            var blocks = db.Hostels.Where(x => x.type.StartsWith(gender));
            List<SelectListItem> blockList = new List<SelectListItem>();
            foreach (var x in blocks)
            {
                blockList.Add(new SelectListItem { Text = x.blockNumber + "", Value = x.blockNumber + "" });
            }
            return Json(blockList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRooms(string block)
        {
            HostelManagementEntities db = new HostelManagementEntities();
            int _block = int.Parse(block);
            var rooms = db.Rooms.Where(x => x.hostelBlockNumber == _block && x.currentOccupancy < x.maxOccupancy);
            List<SelectListItem> roomList = new List<SelectListItem>();
            foreach (var x in rooms)
            {
                roomList.Add(new SelectListItem { Text = x.roomNumber + "", Value = x.roomNumber + "" });
            }
            return Json(roomList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetType(string room)
        {
            HostelManagementEntities db = new HostelManagementEntities();
            int _room = int.Parse(room);
            return Json(db.Rooms.Where(x => x.roomNumber == _room).First().roomType, JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        private void SetUpDropDownViewData()
        {
            HostelManagementEntities db = new HostelManagementEntities();
            List<SelectListItem> genderList = new List<SelectListItem>();
            foreach (var x in db.Genders)
            {
                genderList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
            }
            ViewBag.genderList = genderList;

            List<SelectListItem> courseList = new List<SelectListItem>();
            foreach (var x in db.Courses)
            {
                courseList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
            }
            ViewBag.courseList = courseList;

            List<SelectListItem> departmentList = new List<SelectListItem>();
            foreach (var x in db.Departments)
            {
                departmentList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
            }
            ViewBag.departmentList = departmentList;
        }
    }
}