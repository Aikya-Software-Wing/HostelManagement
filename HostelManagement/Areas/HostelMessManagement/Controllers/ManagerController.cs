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

    }
}