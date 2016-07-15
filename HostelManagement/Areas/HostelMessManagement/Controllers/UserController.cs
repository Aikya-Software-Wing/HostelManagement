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
        private HostelManagementEntities db = new HostelManagementEntities();

        /// <summary>
        /// Action Method to display index
        /// </summary>
        /// <returns>View</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Action Method to log out user
        /// </summary>
        /// <returns>View</returns>
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
        /// Action Method to diplay a page to add a student
        /// </summary>
        /// <returns>View</returns>
        [HttpGet]
        public ActionResult AddStudent()
        {
            // set up the lists required for the drop down
            SetUpDropDownViewData();

            return View();
        }

        /// <summary>
        /// Action Method to add a student to the database
        /// </summary>
        /// <param name="userInput"> the student as defined by the user</param>
        /// <returns>View</returns>
        [HttpPost]
        public ActionResult AddStudent(StudentViewModel userInput)
        {
            // Check if vaild data was enterted
            if (!ModelState.IsValid)
            {
                // set up the lists required for the drop down
                SetUpDropDownViewData();

                return View(userInput);
            }

            // check to see USN was entered when semester > 1
            if (userInput.semester > 1 && string.IsNullOrEmpty(userInput.usn))
            {
                // add model error
                ModelState.AddModelError("usn", "The USN is required");

                // set up the lists required for the drop down
                SetUpDropDownViewData();

                return View(userInput);
            }

            // generate a unique border ID for the student
            string bid = GenerateBid(userInput.branch, userInput.year);

            // create the new student
            Student newStudent = new Student()
            {
                bid = bid,
                name = userInput.name.Trim(),
                usn = string.IsNullOrEmpty(userInput.usn) ? "" : userInput.usn,
                semester = userInput.semester,
                gender = userInput.gender,
                course = userInput.course,
                branch = userInput.branch,
                dob = userInput.dob
            };

            // allot a room to the student
            newStudent.Allotments.Add(new Allotment()
            {
                bid = bid,
                dateOfJoin = userInput.doj,
                hostelBlock = userInput.blockNumber,
                roomNum = userInput.roomNumber,
                year = userInput.year
            });

            // update the database
            Room room = db.Rooms.Where(x => x.roomNumber == userInput.roomNumber && x.hostelBlockNumber == userInput.blockNumber).First();
            ++room.currentOccupancy;
            db.Rooms.Attach(room);
            db.Entry(room).State = EntityState.Modified;
            db.Students.Add(newStudent);
            db.SaveChanges();

            // save the bid for future use
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

        /// <summary>
        /// Method to generate a unique border ID 
        /// </summary>
        /// <param name="department"> the department to which the student belongs to </param>
        /// <param name="year"> the year of admission</param>
        /// <returns>the unique border ID</returns>
        [NonAction]
        private string GenerateBid(int department, int year)
        {
            int numberOfStudent = db.Students.Where(x => x.branch == department && x.bid.ToString().StartsWith(year.ToString().Trim().Substring(2, 2))).ToList().Count + 1;
            return year.ToString().Trim().Substring(2, 2) + db.Departments.Where(x => x.id == department).First().code + numberOfStudent.ToString();
        }

        /// <summary>
        /// Method to return the Fee breakup in JSON format
        /// </summary>
        /// <param name="gender"> the students gender</param>
        /// <param name="roomType"> the type of room allotted to the student</param>
        /// <returns>the fee breakup in JSON format</returns>
        public JsonResult GetFeeBreakUp(string gender, string roomType)
        {
            // get the Database ID for gender and room type
            int gen = db.Genders.Where(x => x.val.Equals(gender)).First().id;
            int typ = db.RoomTypes.Where(x => x.val.Equals(roomType)).First().id;

            // get the daily mess charges from the database
            decimal dailymess = db.HostelCharges.Where(x => x.id == 0).First().val.Value;

            // get the Database ID for the various types of fees
            int rentCode = int.Parse(gen + "" + typ + "1");
            int fixedCode = int.Parse(gen + "" + typ + "2");
            int depCode = int.Parse(gen + "" + typ + "3");

            // get the fees from the database
            decimal rent = db.HostelCharges.Where(x => x.id == rentCode).First().val.Value;
            decimal fix = db.HostelCharges.Where(x => x.id == fixedCode).First().val.Value;
            decimal deposit = db.HostelCharges.Where(x => x.id == depCode).First().val.Value;

            // add the fees to the list
            List<decimal> charges = new List<decimal>();
            charges.Add(dailymess);
            charges.Add(rent);
            charges.Add(fix);
            charges.Add(deposit);

            // return the list in JSON format
            return Json(charges, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Action Method to return a list of hostel blocks that the student can occupy based on gender
        /// </summary>
        /// <param name="gender"> the students gender</param>
        /// <returns>the list of hostel blocks in JSON format</returns>
        public JsonResult GetBlocks(string gender)
        {
            List<SelectListItem> blockList = new List<SelectListItem>();

            // get the hostel blocks that can be allotted based on the students gender
            try
            {
                int genderId = db.Genders.Where(x => x.val.Equals(gender)).First().id;
                var blocks = db.Hostels.Where(x => x.occupantType == genderId);
                foreach (var x in blocks)
                {
                    blockList.Add(new SelectListItem { Text = x.blockNumber + "", Value = x.blockNumber + "" });
                }
            }
            catch (InvalidOperationException)
            {
            }

            // return the list in JSON format
            return Json(blockList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Action method to get vacant rooms in a block
        /// </summary>
        /// <param name="block"> the hostel block</param>
        /// <returns>the list of rooms in JSON format</returns>
        public JsonResult GetRooms(string block)
        {
            List<SelectListItem> roomList = new List<SelectListItem>();

            // convert the block number from string to int
            int _block = int.Parse(block);

            // find all rooms in the block where the is space for occupancy
            var rooms = db.Rooms.Where(x => x.hostelBlockNumber == _block && x.currentOccupancy < x.maxOccupancy);

            // add the above found rooms to a list that can by displayed
            foreach (var x in rooms)
            {
                roomList.Add(new SelectListItem { Text = x.roomNumber + "", Value = x.roomNumber + "" });
            }

            // return the list in JSON format
            return Json(roomList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Action Method to get the type of room
        /// </summary>
        /// <param name="room"> the room number</param>
        /// <param name="blockNumber"> the block number where the room is located</param>
        /// <returns> the type of room</returns>
        public JsonResult GetType(string room, string blockNumber)
        {
            // convert the room number to int
            int _room = int.Parse(room);

            // convert the block number to int
            int _block = int.Parse(blockNumber);

            // retrun the type of room
            return Json(db.Rooms.Where(x => x.roomNumber == _room && x.hostelBlockNumber == _block).First().RoomType1.val, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Method to set up the lists required for the drop down
        /// </summary>
        [NonAction]
        private void SetUpDropDownViewData()
        {
            // set up the list of genders
            List<SelectListItem> genderList = new List<SelectListItem>();
            foreach (var x in db.Genders)
            {
                genderList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
            }
            ViewBag.genderList = genderList;

            // set the list of courses
            List<SelectListItem> courseList = new List<SelectListItem>();
            foreach (var x in db.Courses)
            {
                courseList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
            }
            ViewBag.courseList = courseList;

            // set up list of departments
            List<SelectListItem> departmentList = new List<SelectListItem>();
            foreach (var x in db.Departments)
            {
                departmentList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
            }
            ViewBag.departmentList = departmentList;
        }
    }
}