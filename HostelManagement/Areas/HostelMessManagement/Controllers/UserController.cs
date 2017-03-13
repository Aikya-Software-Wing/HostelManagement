using BusinessLayer;
using DevExpress.XtraPrinting;
using HostelManagement.Areas.HostelMessManagement.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
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
                TempData["bid"] = student.bid;
                return JavaScript("window.location='"+Url.Action("HostelTransaction")+"'");
                //return Content("Student added successfully, bid = " + student.bid);
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
            StudentSearchViewModel model = new StudentSearchViewModel
            {
                bid = TempData.Peek("bid") == null ? "" : (string)TempData.Peek("bid")
            };

            return View(model);
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

        [HttpGet]
        public ActionResult EditHostelTransaction(int id)
        {
            TransactionHelper helper = new TransactionHelper();
            HostelTransactionViewModel viewModel = helper.GetHotelTransactionViewModel(id);

            // generate the list of account heads to be displayed and add it to the view bag
            ViewBag.acHeadList = new SelectList(helper.GetAccountHeads(), "id", "val");

            // generate the list of payment types to be displayed and add it to the view bag
            ViewBag.paymentTypeList = new SelectList(helper.GetPaymentTypes(false), "id", "val");

            //generate the list of academic years to be displayed and add it to the view bag
            ViewBag.academicYearList = new SelectList(helper.GetValidAcademicYears(viewModel.year), DateTime.Now.Year + "");

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult EditHostelTransaction(HostelTransactionViewModel userInput)
        {
            if(!ModelState.IsValid)
            {
                return View(userInput);
            }

            TransactionHelper helper = new TransactionHelper();
            helper.EditHotelTransaction(userInput);
            TempData["bid"] = userInput.bid;

            return RedirectToAction("HostelTransaction");
        }

        public ActionResult EditMessTransaction(int id)
        {
            TransactionHelper helper = new TransactionHelper();
            MessTransactionViewModel viewModel = helper.GetMessTransactionViewModel(id);

            // generate the list of account heads to be displayed and add it to the view bag
            ViewBag.acHeadList = new SelectList(helper.GetAccountHeads(), "id", "val");

            // generate the list of payment types to be displayed and add it to the view bag
            ViewBag.paymentTypeList = new SelectList(helper.GetPaymentTypes(false), "id", "val");

            //generate the list of academic years to be displayed and add it to the view bag
            ViewBag.academicYearList = new SelectList(helper.GetValidAcademicYears(viewModel.year));

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult EditMessTransaction(MessTransactionViewModel userInput)
        {
            if (!ModelState.IsValid)
            {
                return View(userInput);
            }

            TransactionHelper helper = new TransactionHelper();
            helper.EditMessTransaction(userInput);
            TempData["bid"] = userInput.bid;

            return RedirectToAction("MessTransaction");
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
            StudentSearchViewModel model = new StudentSearchViewModel
            {
                bid = TempData.Peek("bid") == null ? "" : (string)TempData.Peek("bid")
            };

            return View(model);
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

        /// <summary>
        /// Action method to display a form to select the type of report
        /// </summary>
        /// <returns>a view</returns>
        public ActionResult GenerateReport()
        {
            SetUpDropDown();
            return View();
        }

        /// <summary>
        /// Action method to get the form to filled after selecting the report type
        /// </summary>
        /// <param name="reportType">the type of report</param>
        /// <returns>a partial view or error message</returns>
        [HttpPost]
        public ActionResult GenerateReport(string reportType)
        {
            StudentHelper studentHelper = new StudentHelper();
            TransactionHelper transactionHelper = new TransactionHelper();

            // save the report type for later use
            TempData["queryField"] = reportType;

            // switch based on report type
            switch (reportType)
            {
                case "1":
                    return PartialView("_ReportByBID");
                case "2":
                    ViewBag.dataList = new SelectList(studentHelper.GetGenders(), "id", "val");
                    return PartialView("_ReportByDropDown");
                case "3":
                    // generate a list for the semesters
                    List<SelectListItem> temp = new List<SelectListItem>();
                    for (int i = 1; i <= 8; i++)
                    {
                        temp.Add(new SelectListItem { Text = i + "", Value = i + "" });
                    }
                    ViewBag.dataList = new SelectList(temp, "Value", "Text");

                    return PartialView("_ReportByDropDown");
                case "4":
                    ViewBag.dataList = new SelectList(studentHelper.GetCourses(), "id", "val");

                    return PartialView("_ReportByDropDown");
                case "5":
                    return PartialView("_ReportByDate");
                case "6":
                    ViewBag.dataList = new SelectList(transactionHelper.GetPaymentTypes(true), "id", "val");

                    return PartialView("_ReportByDropDown");
                case "7":
                    ViewBag.dataList = new SelectList(transactionHelper.GetAccountHeads(), "id", "val");

                    return PartialView("_ReportByDropDown");
                case "8":
                    return PartialView("_ReportByAmount");
            }
            return Content("An error occurred");
        }

        /// <summary>
        /// Action method to generate the report using BID ranges
        /// </summary>
        /// <param name="StartBID">the start BID range</param>
        /// <param name="EndBID">the end BID range</param>
        /// <returns>the report (excel file) or error message</returns>
        public ActionResult GenerateReportByBID(string[] StartBID, string[] EndBID)
        {
            ReportHelper helper = new ReportHelper();
            List<string> startStudent, endStudent;

            // get only valid ranges
            helper.ExtractValidRanges(StartBID, EndBID, out startStudent, out endStudent);

            // if no valid ranges could be found
            if (startStudent.Count <= 0)
            {
                SetUpDropDown();
                ModelState.AddModelError("Range", "Enter a valid range");
                return View("GenerateReport");
            }

            // get the transactions
            List<TransactionsViewModel> viewModel = helper.GetTransactionsByStudentRange(startStudent, endStudent);

            // generate excel file
            var stream = helper.GenerateExcel(viewModel);

            // set the properties for the excel file
            DateTime now = DateTime.Now;
            string fileName = "Report - " + now.ToLongDateString() + " " + now.ToLongTimeString() + ".xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // reset the stream position (for safety purposes)
            stream.Position = 0;

            return File(stream, contentType, fileName);
        }

        /// <summary>
        /// Action method to generate report based on value
        /// </summary>
        /// <param name="value">the selected value</param>
        /// <returns>the report or a error message</returns>
        public ActionResult GenerateReportByDropDown(string value)
        {
            ReportHelper helper = new ReportHelper();
            List<TransactionsViewModel> viewModel = null;

            // generate the report based on the previously saved report type
            switch (TempData.Peek("queryField").ToString())
            {
                case "2":
                    viewModel = helper.GetTransactionsByGender(int.Parse(value));
                    break;
                case "3":
                    viewModel = helper.GetTransactionsBySem(int.Parse(value));
                    break;
                case "4":
                    viewModel = helper.GetTransactionsByCourse(int.Parse(value));
                    break;
                case "6":
                    viewModel = helper.GetTransactionsByPaymentType(int.Parse(value));
                    break;
                case "7":
                    viewModel = helper.GetTransactionsByAccountHead(int.Parse(value));
                    break;
            }

            // generate the excel file
            var stream = helper.GenerateExcel(viewModel);

            // set the property for the excel file
            DateTime now = DateTime.Now;
            string fileName = "Report - " + now.ToLongDateString() + " " + now.ToLongTimeString() + ".xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // reset the stream position (for safety)
            stream.Position = 0;

            return File(stream, contentType, fileName);
        }

        /// <summary>
        /// Action method to generate a report based on date range
        /// </summary>
        /// <param name="StartDate">the start date</param>
        /// <param name="EndDate">the end date</param>
        /// <returns>the report</returns>
        public ActionResult GenerateReportByDate(DateTime StartDate, DateTime EndDate)
        {
            ReportHelper helper = new ReportHelper();

            // sanity check
            if (StartDate > EndDate)
            {
                SetUpDropDown();
                ModelState.AddModelError("Range", "Enter a valid range");
                return View("GenerateReport");
            }

            // get the transactions
            List<TransactionsViewModel> viewModel = helper.GetTransactionsByDateRange(StartDate, EndDate);

            // generate the excel file
            var stream = helper.GenerateExcel(viewModel);

            // set the properties of the excel file
            DateTime now = DateTime.Now;
            string fileName = "Report - " + now.ToLongDateString() + " " + now.ToLongTimeString() + ".xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // reset the stream position (for safety purpose)
            stream.Position = 0;

            return File(stream, contentType, fileName);
        }

        /// <summary>
        /// Action method to generate report by amount range
        /// </summary>
        /// <param name="StartAmt">the start amount</param>
        /// <param name="EndAmt">the end amount</param>
        /// <returns>the report or an error message</returns>
        public ActionResult GenerateReportByAmount(decimal StartAmt, decimal EndAmt)
        {
            ReportHelper helper = new ReportHelper();

            // sanity check
            if (StartAmt > EndAmt)
            {
                SetUpDropDown();
                ModelState.AddModelError("Range", "Enter a valid range");
                return View("GenerateReport");
            }

            // get the transactions
            List<TransactionsViewModel> viewModel = helper.GetTransactionsByAmountRange(StartAmt, EndAmt);

            // generate the excel file
            var stream = helper.GenerateExcel(viewModel);

            // set the properties for the excel file
            DateTime now = DateTime.Now;
            string fileName = "Report - " + now.ToLongDateString() + " " + now.ToLongTimeString() + ".xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // reset the stream position (for safety purposes)
            stream.Position = 0;

            return File(stream, contentType, fileName);
        }

        /// <summary>
        /// Action method to display form that user has to fill to generate individual student report
        /// </summary>
        /// <returns>a view</returns>
        [HttpGet]
        public ActionResult GenerateStudentReport()
        {
            return View();
        }

        /// <summary>
        /// Action Method to generate student report
        /// </summary>
        /// <param name="userInput">the student</param>
        /// <returns>a pdf file</returns>
        [HttpPost]
        public ActionResult GenerateStudentReport(StudentSearchViewModel userInput)
        {
            // find the student, if not found, return error
            StudentHelper helper = new StudentHelper();
            var studentInfo = helper.GetStudent(userInput.bid);
            if (studentInfo == null)
            {
                ModelState.AddModelError("", "Student not found!");
                return View(userInput);
            }

            StudentReportDataSet studentDataSet = new StudentReportDataSet();

            // add data to student table
            DataRow myDataRow = studentDataSet.Tables["Student"].NewRow();
            myDataRow["Name"] = studentInfo.name;
            myDataRow["Sem"] = studentInfo.semester;
            myDataRow["Gender"] = studentInfo.Gender1.val;
            myDataRow["Course"] = studentInfo.Course1.val;
            myDataRow["Branch"] = studentInfo.Department.val;
            myDataRow["DateOfBirth"] = studentInfo.dob.ToLongDateString();
            studentDataSet.Tables["Student"].Rows.Add(myDataRow);

            // add data to allotment table
            foreach (var allotment in studentInfo.Allotments)
            {
                myDataRow = studentDataSet.Tables["Allotment"].NewRow();
                myDataRow["DateOfJoin"] = allotment.dateOfJoin.ToLongDateString();
                myDataRow["DateOfLeave"] = allotment.dateOfLeave.HasValue ? allotment.dateOfLeave.Value.ToLongDateString() : "";
                myDataRow["HostelBlock"] = allotment.hostelBlock;
                myDataRow["RoomNumber"] = allotment.roomNum;
                studentDataSet.Tables["Allotment"].Rows.Add(myDataRow);
            }

            // get data for hostel fee table
            TransactionHelper helper1 = new TransactionHelper();
            Tuple<List<HostelFeeDueViewModel>, Hashtable> result = helper1.GetStudentDues(userInput.bid, true);
            List<HostelFeeDueViewModel> viewModel = result.Item1;

            // add data to hostel fee table
            foreach (var item in viewModel)
            {
                if (item.amount != 0)
                {
                    myDataRow = studentDataSet.Tables["HostelFee"].NewRow();
                    myDataRow["Year"] = item.academicYear;
                    myDataRow["Fee Type"] = item.accountHead;
                    myDataRow["Fee Amount"] = item.amount;
                    myDataRow["Amount Paid"] = item.amountPaid;
                    myDataRow["Amount Due"] = item.amountDue;
                    studentDataSet.Tables["HostelFee"].Rows.Add(myDataRow);
                }
            }

            // get data for mess fee table
            List<MessFeeDueViewModel> viewModel1 = helper1.GetMessDue(userInput.bid, true);

            // add data to mess fee table
            foreach (var item in viewModel1)
            {
                myDataRow = studentDataSet.Tables["MeeFee"].NewRow();
                myDataRow["Year"] = item.academicYear;
                myDataRow["Month"] = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.month);
                myDataRow["Amount"] = item.amount;
                myDataRow["Amount Paid"] = item.amountPaid;
                myDataRow["Amount Due"] = item.amountDue;
                studentDataSet.Tables["MeeFee"].Rows.Add(myDataRow);
            }

            // set data source for the report
            StudentReport studentReport = new StudentReport();
            studentReport.DataSource = studentDataSet;

            // export to pdf, write memory stream to response directly
            using (MemoryStream ms = new MemoryStream())
            {
                PdfExportOptions opts = new PdfExportOptions();
                opts.ShowPrintDialogOnOpen = true;
                studentReport.ExportToPdf(ms, opts);
                ms.Seek(0, SeekOrigin.Begin);
                byte[] report = ms.ToArray();
                Response.ContentType = "application/pdf";
                Response.Clear();
                Response.OutputStream.Write(report, 0, report.Length);
                Response.End();
            }

            return null;
        }

        /// <summary>
        /// Helper method to set the list drop down for report generation
        /// </summary>
        [NonAction]
        private void SetUpDropDown()
        {
            List<SelectListItem> reportTypeList = new List<SelectListItem>();
            reportTypeList.Add(new SelectListItem { Text = "BID", Value = "1" });
            reportTypeList.Add(new SelectListItem { Text = "Gender", Value = "2" });
            reportTypeList.Add(new SelectListItem { Text = "Semester", Value = "3" });
            reportTypeList.Add(new SelectListItem { Text = "Course", Value = "4" });
            reportTypeList.Add(new SelectListItem { Text = "Date", Value = "5" });
            reportTypeList.Add(new SelectListItem { Text = "Payment Type", Value = "6" });
            reportTypeList.Add(new SelectListItem { Text = "Account Head", Value = "7" });
            reportTypeList.Add(new SelectListItem { Text = "Amount", Value = "8" });
            ViewBag.reportTypeList = new SelectList(reportTypeList, "Value", "Text");
        }
    }
}