using HostelManagement.Areas.HostelMessManagement.Models;
using HostelManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HostelManagement.Areas.HostelMessManagement.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private HostelManagementEntities db = new HostelManagementEntities();

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
            // populate the room type list such that the appropriate option is selected
            try
            {
                int roomTypeID = db.RoomTypes.Where(y => y.val.Equals(roomType1)).First().id;
                ViewBag.roomTypeList = new SelectList(db.RoomTypes.ToList(), "id", "val", roomTypeID);
            }
            catch (InvalidOperationException)
            {
                ViewBag.roomTypeList = new SelectList(db.RoomTypes, "id", "val");
            }

            // populate the gender list such that the appropriate option is selected
            try
            {
                int genId = db.Genders.Where(x => x.val.Equals(gender)).First().id;
                ViewBag.hostelTypeList = new SelectList(db.Genders.ToList(), "id", "val", genId);
            }
            catch (InvalidOperationException)
            {
                ViewBag.hostelTypeList = new SelectList(db.Genders, "id", "val");
            }

            // add the mess charges to the model
            ViewBag.messChargesModel = new MessChargesViewModel() { dailymess = db.HostelCharges.Where(x => x.id == 0).First().val.Value};

            return View();
        }

        /// <summary>
        /// Action Method to save the changes (daily mess charges) to the database
        /// </summary>
        /// <param name="userInput"> the fees as updated by the user</param>
        /// <returns>Partial View or Success Message</returns>
        public ActionResult ChangeMessFees(MessChargesViewModel userInput)
        {
            // if model is not valid, do not process furthur
            if (!ModelState.IsValid)
            {
                return PartialView("_MessFeeChange", userInput);
            }
            
            // find the value of the daily mess fees in the database
            decimal originalValue = db.HostelCharges.Where(x => x.id == 0).First().val.Value;

            // if the user has changed the value, update the same in the database
            if(originalValue != userInput.dailymess)
            {
                UpdateDatabaseValue(0, userInput.dailymess);
                return Content("Update Success!!");
            }

            return PartialView("_MessFeeChange", userInput);
        }

        /// <summary>
        /// Action Method to display a form to update the hostel fees
        /// </summary>
        /// <param name="userInput"> the serach query as input by the user</param>
        /// <returns>Partial View</returns>
        public PartialViewResult ChangeFees(SearchViewModel userInput)
        {
            HostelChargesViewModel model = new HostelChargesViewModel();

            // get the ID of the various fees
            int rentId = int.Parse(userInput.hostelType + "" + userInput.roomType + "1");
            int fixId = int.Parse(userInput.hostelType + "" + userInput.roomType + "2");
            int depId = int.Parse(userInput.hostelType + "" + userInput.roomType + "3");

            // get the original values of the fees in the database, if not present, zero fill
            try
            {
                model.rent = db.HostelCharges.Where(x => x.id == rentId).First().val.Value;
                model.fix = db.HostelCharges.Where(x => x.id == fixId).First().val.Value;
                model.deposit = db.HostelCharges.Where(x => x.id == depId).First().val.Value;
            }catch(InvalidOperationException)
            {
                model.rent = model.fix = model.deposit = 0;
            }

            // save the ids for future use
            TempData["originalValues"] = model;
            TempData["rentId"] = rentId;
            TempData["fixId"] = fixId;
            TempData["depId"] = depId;

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

            // find out the items that the user has changed
            bool rentChanged = originalValues.rent != userInput.rent;
            bool fixChanged = originalValues.fix != userInput.fix;
            bool depChanged = originalValues.deposit != userInput.deposit;

            // if user has changed anything
            if(rentChanged || fixChanged || depChanged)
            {
                // get the previously saved IDs
                int rentId = (int) TempData.Peek("rentId");
                int fixId = (int)TempData.Peek("fixId");
                int depId = (int)TempData.Peek("depId");

                // update rent in the database, if changed
                if (rentChanged)
                {
                    UpdateDatabaseValue(rentId, userInput.rent);
                }

                // update fixed changes in the database, if changed
                if(fixChanged)
                {
                    UpdateDatabaseValue(fixId, userInput.fix);
                }

                // update deposit in the database, if changed
                if(depChanged)
                {
                    UpdateDatabaseValue(depId, userInput.deposit);
                }

                // return success message
                return Content("Update Success!!");
            }

            return PartialView("_FeeChange", userInput);
        }

        /// <summary>
        /// Helper method to update values in the database
        /// </summary>
        /// <param name="id"> the ID of the value to the change</param>
        /// <param name="changedValue"> the updated value</param>
        [NonAction]
        public void UpdateDatabaseValue(int id, decimal changedValue)
        {
            HostelCharge value = db.HostelCharges.Where(x => x.id == id).First();
            value.val = changedValue;
            db.HostelCharges.Attach(value);
            db.Entry(value).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}