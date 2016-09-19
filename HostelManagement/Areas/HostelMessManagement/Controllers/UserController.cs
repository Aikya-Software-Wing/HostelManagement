using BusinessLayer;
using HostelManagement.Areas.HostelMessManagement.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HostelManagement.Areas.HostelMessManagement.Controllers
{
    /// <summary>
    /// Controller for User
    /// </summary>
    [Authorize(Roles = "User,Manager")]
    public class UserController : Controller
    {

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
            StudentHelper helper = new StudentHelper();
            string error = "";

            Student student = helper.AddStudent(userInput, out error);
            if (student != null)
            {
                return Content("Student Add Success BID = " + student.bid);
            }

            return Content(error);
        }

        /// <summary>
        /// Method to return the Fee breakup in JSON format
        /// </summary>
        /// <param name="gender"> the students gender</param>
        /// <param name="roomType"> the type of room allotted to the student</param>
        /// <returns>the fee breakup in JSON format</returns>
        public JsonResult GetFeeBreakUp(string gender, string roomType)
        {
            StudentHelper helper = new StudentHelper();
            return Json(helper.GetFeeBreakup(gender, roomType), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Action Method to return a list of hostel blocks that the student can occupy based on gender
        /// </summary>
        /// <param name="gender"> the students gender</param>
        /// <returns>the list of hostel blocks in JSON format</returns>
        public JsonResult GetBlocks(string gender)
        {
            StudentHelper helper = new StudentHelper();
            return Json(new SelectList(helper.GetHostelsForStudent(gender), "blockNumber", "blockNumber"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Action method to get vacant rooms in a block
        /// </summary>
        /// <param name="block"> the hostel block</param>
        /// <returns>the list of rooms in JSON format</returns>
        public JsonResult GetRooms(string block)
        {
            StudentHelper helper = new StudentHelper();
            return Json(new SelectList(helper.GetAvailableRoomsInHostel(int.Parse(block)), "roomNumber", "roomNumber"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Action Method to get the type of room
        /// </summary>
        /// <param name="room"> the room number</param>
        /// <param name="blockNumber"> the block number where the room is located</param>
        /// <returns> the type of room</returns>
        public JsonResult GetType(string room, string blockNumber)
        {
            StudentHelper helper = new StudentHelper();
            return Json(helper.GetRoomType(int.Parse(blockNumber), int.Parse(room)), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Action Method to display the form to add additional fees
        /// </summary>
        /// <returns>view</returns>
        [HttpGet]
        public ActionResult AddAdditionalFee()
        {
            return View();
        }

        /// <summary>
        /// Action Method to actually add the additional fee to the database
        /// </summary>
        /// <param name="userInput">the form filled by user</param>
        /// <returns>view</returns>
        [HttpPost]
        public ActionResult AddAdditionalFee(AddAdditionalFeeViewModel userInput)
        {
            StudentHelper helper = new StudentHelper();

            //if model is not vaild, do not process furthur
            if (!ModelState.IsValid)
            {
                return View();
            }

            return Content(helper.AddAdditionalFee(userInput));
        }

        /// <summary>
        /// Action method to search for the student (for hostel transactions)
        /// </summary>
        /// <returns>view</returns>
        [HttpGet]
        public ActionResult HostelTransaction()
        {
            return View();
        }

        /// <summary>
        /// Action method to display a form to enable the user to perform hostel transactions
        /// </summary>
        /// <param name="userInput">the form as input by the user</param>
        /// <returns>view</returns>
        [HttpPost]
        public ActionResult HostelTransaction(StudentSearchViewModel userInput)
        {
            TransactionHelper helper = new TransactionHelper();
            // make an attempt to find the student and display the form, if not possible, show error
            HostelTransactionViewModel viewModel = helper.ConstructViewModelForHostelTransaction(userInput.bid);

            if (viewModel != null)
            {
                // generate the list of account heads to be displayed and add it to the view bag
                ViewBag.acHeadList = new SelectList(helper.GetAccountHeads(), "id", "val");

                // generate the list of payment types to be displayed and add it to the view bag
                ViewBag.paymentTypeList = new SelectList(helper.GetPaymentTypes(false), "id", "val");

                //generate the list of academic years to be displayed and add it to the view bag
                ViewBag.academicYearList = new SelectList(helper.GetValidAcademicYears(viewModel.year), DateTime.Now.Year + "");

                // display the form
                return PartialView("_AddHostelTransation", viewModel);
            }
            else
            {
                return Content("Student not found");
            }
        }

        /// <summary>
        /// Action Method to get the amount payable
        /// </summary>
        /// <param name="head">the account head</param>
        /// <param name="bid">the border ID</param>
        /// <param name="year">the academic year</param>
        /// <returns>the amount payable in JSON format</returns>
        public ActionResult GetAmount(string head, string bid, string year)
        {
            TransactionHelper helper = new TransactionHelper();
            return Json(helper.GetHostelFeePayable(head, bid, int.Parse(year)), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Action Method to amount payable towards the mess bill
        /// </summary>
        /// <param name="bid">the border ID</param>
        /// <param name="month">the month of the bill</param>
        /// <param name="year">the year of the bill</param>
        /// <returns>the amount payable in JSON format</returns>
        public ActionResult GetMessAmount(string bid, string month, string year)
        {
            TransactionHelper helper = new TransactionHelper();
            // return the amount in JSON format
            return Json(helper.GetMessFeePayable(bid, int.Parse(month), int.Parse(year)), JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Action Method to perform hostel transaction
        /// </summary>
        /// <param name="userInput">the form filled by the user</param>
        /// <returns></returns>
        public ActionResult PerformHostelTransaction(HostelTransactionViewModel userInput)
        {
            TransactionHelper helper = new TransactionHelper();

            // if model is not valid do not process furthur
            if (!ModelState.IsValid)
            {
                // generate the list of account heads to be displayed and add it to the view bag
                ViewBag.acHeadList = new SelectList(helper.GetAccountHeads(), "id", "val");

                // generate the list of payment types to be displayed and add it to the view bag
                ViewBag.paymentTypeList = new SelectList(helper.GetPaymentTypes(false), "id", "val");

                return PartialView("_AddHostelTransation", userInput);
            }

            // add the transaction to the database
            return Content(helper.PerformHostelTransaction(userInput));
        }

        //[HttpGet]
        //public ActionResult HostelDues()
        //{
        //    return View();
        //}

        /// <summary>
        /// Action method to display all hostel dues
        /// </summary>
        /// <returns>view</returns>
        [HttpGet]
        public ActionResult ShowAllDues()
        {
            TransactionHelper helper = new TransactionHelper();
            List<StudentDueSummaryViewModel> viewModel = helper.GetAllHostelDues();
            ViewBag.allTransactions = false;

            return View(viewModel);
        }

        /// <summary>
        /// Action Method to show all mess dues
        /// </summary>
        /// <returns>view</returns>
        public ActionResult ShowAllMessDues()
        {
            TransactionHelper helper = new TransactionHelper();
            List<StudentMessDueSummaryViewModel> viewModel = helper.GetAllMessDues();

            return View(viewModel);
        }

        /// <summary>
        /// Action Method to display the mess dues as a partial view
        /// </summary>
        /// <param name="bid">the students BID</param>
        /// <returns>partial view</returns>
        public ActionResult ShowMessDues(string bid)
        {
            TransactionHelper helper = new TransactionHelper();
            List<MessFeeDueViewModel> viewModel = helper.GetMessDue(bid);
            return PartialView("_MessFeeDueDisplay", viewModel);
        }

        /// <summary>
        /// Action Method to show due for a single student
        /// </summary>
        /// <param name="bid">the students BID</param>
        /// <param name="allTransactions">true, if all transactions are required, false otherwise</param>
        /// <returns></returns>
        public ActionResult ShowDueByStudent(string bid, bool allTransactions = false)
        {
            TransactionHelper helper = new TransactionHelper();
            try
            {
                // get the fee dues 
                Tuple<List<HostelFeeDueViewModel>, Hashtable> result = helper.GetStudentDues(bid, allTransactions);
                List<HostelFeeDueViewModel> viewModel = result.Item1;
                ViewBag.totalDues = result.Item2;

                return PartialView("_FeeDueDisplay", viewModel);
            }
            catch (InvalidOperationException)
            {
                return Content("Student not found");
            }
        }

        /// <summary>
        /// Action Method to show hostel dues
        /// </summary>
        /// <param name="bid">the BID of the student</param>
        /// <returns></returns>
        public ActionResult ShowDues(string bid)
        {
            return ShowDueByStudent(bid);
        }


        /// <summary>
        /// Action method to search for a student to view his or her details
        /// </summary>
        /// <returns>view</returns>
        public ActionResult ViewStudent()
        {
            return View();
        }


        /// <summary>
        /// Action Method to search for a student to change room
        /// </summary>
        /// <returns>view</returns>
        public ActionResult ChangeRoom()
        {
            return View();
        }

        /// <summary>
        /// Action Method to serach for a student to discount fees
        /// </summary>
        /// <returns></returns>
        public ActionResult DiscountFees()
        {
            return View();
        }

        /// <summary>
        /// Action Method to display a from that allows the user to enter the fee discount
        /// </summary>
        /// <param name="userInput">the form filled by the user</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DiscountFees(StudentSearchViewModel userInput)
        {
            TransactionHelper helper = new TransactionHelper();
            string error = "";

            HostelTransactionViewModel viewModel = helper.ConstructViewModelForHostelFeeDiscount(userInput.bid, out error);
            if (viewModel != null)
            {
                // generate the list of account heads to be displayed and add it to the view bag
                ViewBag.acHeadList = new SelectList(helper.GetAccountHeads(), "id", "val");

                // generate the list of payment types to be displayed and add it to the view bag
                ViewBag.paymentTypeList = new SelectList(helper.GetPaymentTypes(true), "id", "val", "4");

                //generate the list of academic years to be displayed and add it to the view bag
                ViewBag.academicYearList = new SelectList(helper.GetValidAcademicYears(viewModel.year), DateTime.Now.Year + "");

                // display the view
                return PartialView("_AddDiscount", viewModel);
            }

            return Content(error);
        }


        /// <summary>
        /// Action method to record the fee discount in the database
        /// </summary>
        /// <param name="userInput">the form filled by the user</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PerformHostelFeeDiscount(HostelTransactionViewModel userInput)
        {
            TransactionHelper helper = new TransactionHelper();
            return Content(helper.PerformFeeDiscount(userInput));
        }


        /// <summary>
        /// Action method to display a button that when clicked will generate the mess bill
        /// </summary>
        /// <returns>view</returns>
        public ActionResult GenerateMessBill()
        {
            TransactionHelper helper = new TransactionHelper();
            ViewBag.canGenerateMessBill = helper.CanGenerateMessBill();

            return View();
        }

        /// <summary>
        /// Action method to actually generate the mess bill
        /// </summary>
        /// <param name="dummy">a dummy parameter to facilitate polymorphism</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GenerateMessBill(int dummy = 0)
        {
            TransactionHelper helper = new TransactionHelper();
            return Content(helper.GenerateMessBill());
        }

        /// <summary>
        /// Action method to search for a student to perform mess transaction
        /// </summary>
        /// <returns>view</returns>
        public ActionResult MessTransaction()
        {
            return View();
        }

        /// <summary>
        /// Action method to perform the mess transaction
        /// </summary>
        /// <param name="userInput">the form filled by the user</param>
        /// <returns>Content</returns>
        public ActionResult PerformMessTransaction(MessTransactionViewModel userInput)
        {
            TransactionHelper helper = new TransactionHelper();
            return Content(helper.PerformMessTransaction(userInput));
        }

        /// <summary>
        /// Action Method to display a form to allow the user to perform mess transactions
        /// </summary>
        /// <param name="userInput">the form filled by the user</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MessTransaction(StudentSearchViewModel userInput)
        {
            string error = "";
            TransactionHelper helper = new TransactionHelper();
            MessTransactionViewModel viewModel = helper.ConstructViewModelForMessTransaction(userInput.bid, out error);
            if (viewModel != null)
            {
                // generate the list of payment types to be displayed and add it to the view bag
                ViewBag.paymentTypeList = new SelectList(helper.GetPaymentTypes(false), "id", "val");

                //generate the list of academic years to be displayed and add it to the view bag
                ViewBag.academicYearList = new SelectList(helper.GetValidAcademicYears(viewModel.year), DateTime.Now.Year + "");

                return PartialView("_AddMessTransaction", viewModel);
            }
            else
            {
                return Content(error);
            }
        }

        /// <summary>
        /// Action Method to display a from to search for the student to report absence
        /// </summary>
        /// <returns></returns>
        public ActionResult ReportAbsenceForMess()
        {
            return View();
        }

        /// <summary>
        /// Action Method to actually report absence
        /// </summary>
        /// <param name="userInput">the form filled by the user</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ReportAbsenceForMess(MessAbsenceViewModel userInput)
        {
            TransactionHelper helper = new TransactionHelper();
            return Content(helper.ReportAbsenceForMess(userInput));
        }

        /// <summary>
        /// Action method to display a form to change the students room
        /// </summary>
        /// <param name="userInput">the form filled by the user</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeRoom(StudentSearchViewModel userInput)
        {
            StudentHelper helper = new StudentHelper();
            TransactionHelper helper1 = new TransactionHelper();
            string error = "";

            if (helper.CanChangeRoom(userInput.bid, out error))
            {
                Student student = helper.GetStudent(userInput.bid);

                // construct the lists required for the dropdown and add them to the view bag
                ViewBag.hostelBlockList = new SelectList(helper.GetHostelsForStudent(student.Gender1.id + ""), "blockNumber", "blockNumber", student.Allotments.OrderByDescending(x => x.year).First().hostelBlock);

                ViewBag.roomNumberList = new SelectList(helper.GetRoomsIncludingCurrent(userInput.bid), "roomNumber", "roomNumber", student.Allotments.OrderByDescending(x => x.year).First().roomNum);
                ViewBag.gender = student.Gender1.id;
                ViewBag.academicYear = helper1.GetAcademicYear(DateTime.Now);
                TempData["bid"] = student.bid;

                return PartialView("_ChangeRoom");
            }
            return Content(error);
        }

        /// <summary>
        /// Action method to change the room 
        /// </summary>
        /// <param name="userInput">the form filled by the user</param>
        /// <returns></returns>
        public ActionResult ChangeRoomAllotment(ChangeRoomViewModel userInput)
        {
            // get the student, current allotment and room
            string bid = (string)TempData.Peek("bid");
            StudentHelper helper = new StudentHelper();
            return Content(helper.PerformRoomChange(userInput, bid));
        }

        /// <summary>
        /// Action method to view the student
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ViewStudent(StudentSearchViewModel userInput)
        {
            StudentHelper helper = new StudentHelper();
            string error = "";
            DisplayStudentViewModel viewModel = helper.GetStudentDetails(userInput.bid, out error);
            if (viewModel != null)
            {
                ViewBag.bid = userInput.bid;
                return PartialView("_ViewStudent", viewModel);
            }
            return Content(error);
        }

        /// <summary>
        /// Method to set up the lists required for the drop down
        /// </summary>
        [NonAction]
        private void SetUpDropDownViewData()
        {
            StudentHelper helper = new StudentHelper();
            // set up the list of genders
            ViewBag.genderList = new SelectList(helper.GetGenders(), "id", "val");

            // set the list of courses
            ViewBag.courseList = new SelectList(helper.GetCourses(), "id", "val");

            // set up list of departments
            ViewBag.departmentList = new SelectList(helper.GetDepartments(), "id", "val"); ;
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

        /// <summary>
        /// Action Method to show all transactions for a given student
        /// </summary>
        /// <param name="bid">The BID of the student</param>
        /// <param name="archive">true, if the student if the student is in the archive, false otherwise</param>
        /// <returns>a partial view that contains all the transaction</returns>
        public ActionResult ShowAllTransactions(string bid, bool archive = false)
        {
            TransactionHelper helper = new TransactionHelper();
            return PartialView("_AllTransactions", helper.GetAllTransactionsForStudent(bid, archive));
        }

        /// <summary>
        /// Action method to disaply the page to search for a student to be removed
        /// </summary>
        /// <returns>a view</returns>
        [HttpGet]
        public ActionResult RemoveStudent()
        {
            return View();
        }

        /// <summary>
        /// Method to get the form to remove a student
        /// </summary>
        /// <param name="userInput">the from filled by the user</param>
        /// <returns>form if the student was found, else error message</returns>
        [HttpPost]
        public ActionResult RemoveStudent(StudentSearchViewModel userInput)
        {
            StudentHelper helper = new StudentHelper();
            string error = "";

            if (helper.CanRemoveStudent(userInput.bid, out error))
            {
                RemoveStudentViewModel viewModel = helper.ConstructViewModelForRemoveStudent(userInput.bid);
                ViewBag.depositRefund = helper.GetDepositRefund(userInput.bid);
                return PartialView("_RemoveStudent", viewModel);
            }

            return Content(error);
        }

        /// <summary>
        /// Action method to remove the student
        /// </summary>
        /// <param name="userInput">the form filled by the user</param>
        /// <returns>message</returns>
        public ActionResult PerformRemoveStudent(RemoveStudentViewModel userInput)
        {
            StudentHelper helper = new StudentHelper();
            return Content(helper.PerformRemoveStudent(userInput));
        }

        /// <summary>
        /// Action method to get the meet the developers page
        /// </summary>
        /// <returns>a view</returns>
        public ActionResult MeetDevelopers()
        {
            return View();
        }
    }
}