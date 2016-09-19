using BusinessLayer;
using HostelManagement.Areas.HostelMessManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace HostelManagement.Areas.HostelMessManagement.Controllers
{
    /// <summary>
    /// Controller for Manager
    /// </summary>
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private HostelManagementEntities1 db = new HostelManagementEntities1();

        /// <summary>
        /// Action Method to display Index
        /// </summary>
        /// <returns>View</returns>
        public ActionResult Index()
        {
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

            // redirect the user
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        /// <summary>
        /// Action Method to serve the form to change fees
        /// </summary>
        /// <param name="gender"> the gender of the student</param>
        /// <param name="roomType1"> the room type of the allotted room</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ChangeFees(string gender, string roomType1)
        {
            StudentHelper helper = new StudentHelper();
            TransactionHelper tHelper = new TransactionHelper();

            // populate the room type list such that the appropriate option is selected
            try
            {
                int roomTypeID = helper.GetRoomTypes().Where(y => y.val.Equals(roomType1)).First().id;
                ViewBag.roomTypeList = new SelectList(db.RoomTypes.ToList(), "id", "val", roomTypeID);
            }
            catch (InvalidOperationException)
            {
                ViewBag.roomTypeList = new SelectList(db.RoomTypes, "id", "val");
            }

            // populate the gender list such that the appropriate option is selected
            try
            {
                int genId = helper.GetGenders().Where(x => x.val.Equals(gender)).First().id;
                ViewBag.hostelTypeList = new SelectList(db.Genders.ToList(), "id", "val", genId);
            }
            catch (InvalidOperationException)
            {
                ViewBag.hostelTypeList = new SelectList(db.Genders, "id", "val");
            }

            // add the mess charges to the model
            ViewBag.messChargesModel = tHelper.ConstructViewModelForMessFeeChange();
            TempData["canChangeMess"] = tHelper.CanChangeMessFees();

            return View();
        }

        /// <summary>
        /// Action Method to save the changes (daily mess charges) to the database
        /// </summary>
        /// <param name="userInput"> the fees as updated by the user</param>
        /// <returns>Partial View or Success Message</returns>
        public ActionResult ChangeMessFees(MessChargesViewModel userInput)
        {
            TransactionHelper helper = new TransactionHelper();
            // if model is not valid, do not process furthur
            if (!ModelState.IsValid)
            {
                return PartialView("_MessFeeChange", userInput);
            }

            return Content(helper.ChangeMessFees(userInput));
        }

        /// <summary>
        /// Action Method to display a form to update the hostel fees
        /// </summary>
        /// <param name="userInput"> the serach query as input by the user</param>
        /// <returns>Partial View</returns>
        public ActionResult ChangeFees(SearchViewModel userInput)
        {
            TransactionHelper helper = new TransactionHelper();

            HostelChargesViewModel model = helper.ConstructViewModelForHostelFeeChange(userInput);

            TempData["canRentChange"] = helper.CanChangeRent(userInput.hostelType, userInput.roomType);
            TempData["canFixChange"] = helper.CanChangeFix(userInput.hostelType, userInput.roomType);
            TempData["canDepositChange"] = helper.CanChangeDep(userInput.hostelType, userInput.roomType);

            TempData["originalValues"] = model;
            TempData["rentId"] = helper.GetRentFeeId(userInput.hostelType, userInput.roomType); ;
            TempData["fixId"] = helper.GetFixFeeId(userInput.hostelType, userInput.roomType); ;
            TempData["depId"] = helper.GetDepFeeId(userInput.hostelType, userInput.roomType); ;
            return PartialView("_FeeChange", model);
        }

        /// <summary>
        /// Action Method to save the changes to the fees made by the user
        /// </summary>
        /// <param name="userInput"> the changes made to the fees by the user</param>
        /// <returns>PartialView or Success Message</returns>
        public ActionResult UpdateFees(HostelChargesViewModel userInput)
        {
            // if model is not valid, do not process furthur
            if (!ModelState.IsValid)
            {
                return PartialView("_FeeChange", userInput);
            }

            // get previously saved value
            HostelChargesViewModel originalValues = TempData.Peek("originalValues") as HostelChargesViewModel;

            // get the previously saved IDs
            int rentId = (int)TempData.Peek("rentId");
            int fixId = (int)TempData.Peek("fixId");
            int depId = (int)TempData.Peek("depId");

            TransactionHelper helper = new TransactionHelper();

            return Content(helper.ChangeHostelFees(userInput, originalValues, rentId, fixId, depId));
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