using HostelManagement.Areas.HostelMessManagement.Models;
using HostelManagement.Models;
using System;
using System.Collections;
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
            decimal rent = db.HostelCharges.Where(x => x.id == rentCode).OrderByDescending(x => x.year).First().val.Value;
            decimal fix = db.HostelCharges.Where(x => x.id == fixedCode).OrderByDescending(x => x.year).First().val.Value;
            decimal deposit = db.HostelCharges.Where(x => x.id == depCode).OrderByDescending(x => x.year).First().val.Value;

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

        [HttpGet]
        public ActionResult AddAdditionalFee()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddAdditionalFee(AddAdditionalFeeViewModel userInput)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            List<Student> studentList = db.Students.Where(x => x.bid == userInput.bid).ToList();
            if (studentList.Count > 0)
            {
                Student student = studentList.First();
                int yearOfJoining = student.Allotments.OrderByDescending(x => x.year).First().year;
                if (userInput.year >= yearOfJoining && userInput.year <= DateTime.Now.Year)
                {
                    db.HostelBills.Add(new HostelBill()
                    {
                        bid = student.bid,
                        amount = userInput.amount,
                        year = userInput.year,
                        descr = userInput.description
                    });
                    db.SaveChanges();
                }
                else
                {
                    ModelState.AddModelError("year", "Please enter a valid year");
                }
            }
            else
            {
                ModelState.AddModelError("bid", "Student not Found");
            }
            return View();
        }

        [HttpGet]
        public ActionResult HostelTransaction()
        {
            return View();
        }

        [HttpPost]
        public ActionResult HostelTransaction(StudentSearchViewModel userInput)
        {
            try
            {
                Student student = db.Students.Where(x => x.bid == userInput.bid).First();
                Allotment allotment = db.Allotments.Where(x => x.bid == userInput.bid).OrderByDescending(x => x.year).First();
                Room room = db.Rooms.Where(x => x.hostelBlockNumber == allotment.hostelBlock && x.roomNumber == allotment.roomNum).First();
                HostelTransactionViewModel viewModel = new HostelTransactionViewModel()
                {
                    bid = student.bid,
                    name = student.name,
                    dob = student.dob,
                    semester = student.semester,
                    usn = student.usn,
                    gender = db.Genders.Where(x => x.id == student.gender).First().val,
                    course = db.Courses.Where(x => x.id == student.course).First().val,
                    branch = db.Departments.Where(x => x.id == student.branch).First().val,
                    blockNumber = allotment.hostelBlock,
                    roomNumber = allotment.roomNum,
                    roomType = db.RoomTypes.Where(x => x.id == room.roomType).First().val,
                    floorNumber = int.Parse(room.roomNumber.ToString().Substring(0, 1)),
                    doj = allotment.dateOfJoin,
                    year = allotment.year,
                    academicYear = DateTime.Now.Year,
                    dateOfPayment = DateTime.Now.Date
                };
                List<SelectListItem> acHeadList = new List<SelectListItem>();
                foreach (var x in db.AcHeads)
                {
                    acHeadList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
                }
                ViewBag.acHeadList = acHeadList;
                List<SelectListItem> paymentTypeList = new List<SelectListItem>();
                foreach (var x in db.PaymentTypes)
                {
                    paymentTypeList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
                }
                ViewBag.paymentTypeList = paymentTypeList;
                List<SelectListItem> academicYearList = new List<SelectListItem>();
                for (int i = allotment.year; i <= DateTime.Now.Year; i++)
                {
                    academicYearList.Add(new SelectListItem { Text = i + "", Value = i + "" });
                }
                ViewBag.academicYearList = new SelectList(academicYearList, "Value", "Text", DateTime.Now.Year + "");
                return PartialView("_AddHostelTransation", viewModel);
            }
            catch (InvalidOperationException)
            {
                return Content("Student not found");
            }
        }


        public ActionResult GetAmount(string head, string bid, string year)
        {
            int _year = int.Parse(year);
            Tuple<List<HostelFeeDueViewModel>, Hashtable> result = GetStudentDues(bid);
            Hashtable totalDues = result.Item2;
            List<HostelFeeDueViewModel> dues = result.Item1;
            decimal due = dues.Where(x => x.accountHead.StartsWith(head) && x.academicYear == _year).Sum(x => x.amountDue);
            return Json(due, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMessAmount(string bid, string month, string year)
        {
            int _year = int.Parse(year), _month = int.Parse(month);
            decimal amountDue = GetMessDue(bid).Where(x => x.academicYear == _year && x.month == _month).First().amountDue;
            return Json(amountDue, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PerformHostelTransaction(HostelTransactionViewModel userInput)
        {
            if (!ModelState.IsValid)
            {
                List<SelectListItem> acHeadList = new List<SelectListItem>();
                foreach (var x in db.AcHeads)
                {
                    acHeadList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
                }
                ViewBag.acHeadList = acHeadList;
                List<SelectListItem> paymentTypeList = new List<SelectListItem>();
                foreach (var x in db.PaymentTypes)
                {
                    paymentTypeList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
                }
                ViewBag.paymentTypeList = paymentTypeList;

                return PartialView("_AddHostelTransation", userInput);
            }

            db.HostelTransactions.Add(new HostelTransaction()
            {
                head = userInput.acHead,
                bankName = userInput.bankName,
                bid = userInput.bid,
                year = userInput.academicYear,
                receipt = userInput.referenceNumber,
                dateOfPay = userInput.dateOfPayment,
                paymentTypeId = userInput.paymentType,
                amount = userInput.amount
            });
            db.SaveChanges();
            return Content("Success!");
        }

        [HttpGet]
        public ActionResult HostelDues()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ShowAllDues()
        {
            List<AcHead> chargesList = db.AcHeads.ToList();
            List<string> bidList = db.Students.Select(x => x.bid).ToList();
            List<StudentDueSummaryViewModel> viewModel = new List<StudentDueSummaryViewModel>();
            foreach (string bid in bidList)
            {
                Tuple<List<HostelFeeDueViewModel>, Hashtable> result = GetStudentDues(bid);
                Hashtable totalDues = result.Item2;
                bool noDues = true;
                foreach (Tuple<string, decimal> due in totalDues.Values)
                {
                    if (due.Item2 != 0)
                    {
                        noDues = false;
                        break;
                    }
                }
                if (!noDues)
                {
                    viewModel.Add(new StudentDueSummaryViewModel()
                    {
                        bid = bid,
                        rent = ((Tuple<string, decimal>)totalDues[1]).Item2,
                        fix = ((Tuple<string, decimal>)totalDues[2]).Item2,
                        deposit = ((Tuple<string, decimal>)totalDues[3]).Item2,
                        other = ((Tuple<string, decimal>)totalDues[4]).Item2
                    });
                }
            }
            ViewBag.allTransactions = false;
            return View(viewModel);
        }

        [NonAction]
        public Tuple<List<HostelFeeDueViewModel>, Hashtable> GetStudentDues(string bid, bool allTransactions = false)
        {
            Student student = db.Students.Where(x => x.bid == bid).First();
            List<HostelFeeDueViewModel> viewModel = new List<HostelFeeDueViewModel>();
            Hashtable totalDues = new Hashtable();
            Hashtable recurrCost = new Hashtable();
            int yearOfJoining = student.Allotments.OrderBy(x => x.year).First().year;

            for (int i = yearOfJoining; i <= DateTime.Now.Year; i++)
            {
                Allotment allot = student.Allotments.Where(x => x.year <= i).OrderByDescending(x => x.year).First();
                List<AcHead> chargesList = db.AcHeads.ToList();
                int roomType = allot.Room.roomType.Value;

                foreach (AcHead head in chargesList)
                {
                    int id = int.Parse(student.gender + "" + roomType + "" + head.id);
                    decimal amount = 0, amountPaid = 0;
                    List<HostelTransaction> transactions = db.HostelTransactions.Where(x => x.bid == student.bid && x.year == i && x.head == head.id).ToList();
                    if (head.recurr == 1)
                    {
                        if (i != yearOfJoining)
                        {
                            decimal amountPrevious = (decimal)recurrCost[head.id];
                            decimal current = GetFee(id, i, bid, head.custom.Value);
                            amount = current - amountPrevious;
                            recurrCost[head.id] = (decimal)recurrCost[head.id] + amount;
                        }
                        else
                        {
                            amount = GetFee(id, i, bid, head.custom.Value);
                            recurrCost.Add(head.id, amount);
                        }
                    }
                    else
                    {
                        amount = GetFee(id, i, bid, head.custom.Value);
                    }
                    amountPaid = transactions.Sum(x => x.amount).Value;
                    if (totalDues.ContainsKey(head.id))
                    {
                        Tuple<string, decimal> temp = ((Tuple<string, decimal>)totalDues[head.id]);
                        totalDues[head.id] = new Tuple<string, decimal>(temp.Item1, temp.Item2 + (amount - amountPaid));
                    }
                    else
                    {
                        totalDues.Add(head.id, new Tuple<string, decimal>(head.val, amount - amountPaid));
                    }
                    string customAcHead = "";
                    if (head.custom == 1)
                    {
                        List<HostelBill> bills = student.HostelBills.Where(x => x.year == i).ToList();
                        foreach (HostelBill bill in bills)
                        {
                            customAcHead += bill.descr + ",";
                        }
                        if (customAcHead.Length > 0)
                        {
                            customAcHead = customAcHead.Substring(0, customAcHead.Length - 1);
                        }
                    }

                    if (allTransactions)
                    {
                        viewModel.Add(new HostelFeeDueViewModel()
                        {
                            academicYear = i,
                            accountHead = head.custom == 1 ? head.val + "(" + customAcHead + ")" : head.val,
                            amount = amount,
                            amountPaid = amountPaid,
                            amountDue = amount - amountPaid,
                        });
                    }
                    else
                    {
                        if (amountPaid - amount != 0)
                        {
                            viewModel.Add(new HostelFeeDueViewModel()
                            {
                                academicYear = i,
                                accountHead = head.custom == 1 ? head.val + "(" + customAcHead + ")" : head.val,
                                amount = amount,
                                amountPaid = amountPaid,
                                amountDue = amount - amountPaid,
                            });
                        }
                    }
                }
            }
            return new Tuple<List<HostelFeeDueViewModel>, Hashtable>(viewModel, totalDues);
        }


        [NonAction]
        public List<MessFeeDueViewModel> GetMessDue(string bid, bool allTransactions = false)
        {
            Student student = db.Students.Where(x => x.bid == bid).First();
            DateTime dateOfJoin = student.Allotments.OrderBy(x => x.year).First().dateOfJoin;
            List<MessFeeDueViewModel> dues = new List<MessFeeDueViewModel>();
            for(DateTime date = dateOfJoin; date <= DateTime.Now; date = date.AddMonths(1))
            {
                List<MessBill> bills = db.MessBills.Where(x => x.bid == bid && x.month == date.Month && x.dateOfDeclaration.Year == date.Year).ToList();
                if (bills.Count > 0)
                {
                    MessBill bill = bills.First();
                    decimal messChargesPerDay = db.HostelCharges.Where(x => x.id == 0 && x.year <= date.Year).OrderByDescending(x => x.year).First().val.Value;
                    decimal amountPaid = bill.MessTransactions.Sum(x => x.amount).HasValue ? bill.MessTransactions.Sum(x => x.amount).Value : 0;
                    if (allTransactions)
                    {
                        dues.Add(new MessFeeDueViewModel
                        {
                            academicYear = date.Year,
                            amount = messChargesPerDay * bill.numDays,
                            amountPaid = amountPaid,
                            amountDue = (messChargesPerDay * bill.numDays) - (amountPaid),
                            month = date.Month,
                            numberOfDays = bill.numDays
                        });
                    }else
                    {
                        if (amountPaid != (messChargesPerDay * bill.numDays))
                        {
                            dues.Add(new MessFeeDueViewModel
                            {
                                academicYear = date.Year,
                                amount = messChargesPerDay * bill.numDays,
                                amountPaid = amountPaid,
                                amountDue = (messChargesPerDay * bill.numDays) - (amountPaid),
                                month = date.Month,
                                numberOfDays = bill.numDays
                            });
                        }
                    }
                }
            }
            return dues;
        }


        public ActionResult ShowMessDues(string bid)
        {
            List<MessFeeDueViewModel> viewModel = GetMessDue(bid);
            return PartialView("_MessFeeDueDisplay", viewModel);
        }


        public ActionResult ShowAllStudentHostelTransactions()
        {
            List<AcHead> chargesList = db.AcHeads.ToList();
            List<string> bidList = db.Students.Select(x => x.bid).ToList();
            List<StudentDueSummaryViewModel> viewModel = new List<StudentDueSummaryViewModel>();
            foreach (string bid in bidList)
            {
                Tuple<List<HostelFeeDueViewModel>, Hashtable> result = GetStudentDues(bid, true);
                Hashtable totalDues = result.Item2;
                viewModel.Add(new StudentDueSummaryViewModel()
                {
                    bid = bid,
                    rent = ((Tuple<string, decimal>)totalDues[1]).Item2,
                    fix = ((Tuple<string, decimal>)totalDues[2]).Item2,
                    deposit = ((Tuple<string, decimal>)totalDues[3]).Item2,
                    other = ((Tuple<string, decimal>)totalDues[4]).Item2
                });
            }
            ViewBag.allTransactions = true;
            return View("ShowAllDues", viewModel);
        }

        public ActionResult ShowDueByStudent(string bid, bool allTransactions = false)
        {
            try
            {
                Tuple<List<HostelFeeDueViewModel>, Hashtable> result = GetStudentDues(bid, allTransactions);
                List<HostelFeeDueViewModel> viewModel = result.Item1;
                ViewBag.totalDues = result.Item2;
                return PartialView("_FeeDueDisplay", viewModel);
            }
            catch (InvalidOperationException)
            {
                return Content("Student not found");
            }
        }

        public ActionResult ShowDues(string bid)
        {
            return ShowDueByStudent(bid);
        }

        public ActionResult ViewStudent()
        {
            return View();
        }

        public ActionResult ChangeRoom()
        {
            return View();
        }

        public ActionResult DiscountFees()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DiscountFees(StudentSearchViewModel userInput)
        {
            List<Student> studentList = db.Students.Where(x => x.bid == userInput.bid).ToList();
            if(studentList.Count > 0)
            {
                Student student = studentList.First();
                Allotment allotment = db.Allotments.Where(x => x.bid == userInput.bid).OrderByDescending(x => x.year).First();
                Room room = db.Rooms.Where(x => x.hostelBlockNumber == allotment.hostelBlock && x.roomNumber == allotment.roomNum).First();
                HostelTransactionViewModel viewModel = new HostelTransactionViewModel()
                {
                    bid = student.bid,
                    name = student.name,
                    dob = student.dob,
                    semester = student.semester,
                    usn = student.usn,
                    gender = db.Genders.Where(x => x.id == student.gender).First().val,
                    course = db.Courses.Where(x => x.id == student.course).First().val,
                    branch = db.Departments.Where(x => x.id == student.branch).First().val,
                    blockNumber = allotment.hostelBlock,
                    roomNumber = allotment.roomNum,
                    roomType = db.RoomTypes.Where(x => x.id == room.roomType).First().val,
                    floorNumber = int.Parse(room.roomNumber.ToString().Substring(0, 1)),
                    doj = allotment.dateOfJoin,
                    year = allotment.year,
                    academicYear = DateTime.Now.Year,
                    dateOfPayment = DateTime.Now.Date,
                    referenceNumber = "DS",
                    bankName = "RNSIT",
                    paymentType = 4
                };
                List<SelectListItem> acHeadList = new List<SelectListItem>();
                foreach (var x in db.AcHeads)
                {
                    acHeadList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
                }
                ViewBag.acHeadList = acHeadList;
                List<SelectListItem> paymentTypeList = new List<SelectListItem>();
                foreach (var x in db.PaymentTypes)
                {
                    paymentTypeList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
                }
                ViewBag.paymentTypeList = new SelectList(paymentTypeList, "Value", "Text", "4");

                List<SelectListItem> academicYearList = new List<SelectListItem>();
                for (int i = allotment.year; i <= DateTime.Now.Year; i++)
                {
                    academicYearList.Add(new SelectListItem { Text = i + "", Value = i + "" });
                }
                ViewBag.academicYearList = new SelectList(academicYearList, "Value", "Text", DateTime.Now.Year + "");
                return PartialView("_AddDiscount", viewModel);
            }
            return Content("Student not Found!");
        }

        [HttpPost]
        public ActionResult PerformHostelFeeDiscount(HostelTransactionViewModel userInput)
        {
            db.HostelTransactions.Add(new HostelTransaction()
            {
                head = userInput.acHead,
                bankName = userInput.bankName,
                bid = userInput.bid,
                year = userInput.academicYear,
                receipt = userInput.referenceNumber,
                dateOfPay = userInput.dateOfPayment,
                paymentTypeId = 4,
                amount = userInput.amount
            });
            db.SaveChanges();
            return Content("Success!");
        }

        public ActionResult GenerateMessBill()
        {
            int month = DateTime.Now.AddMonths(-1).Month;
            bool canGenerateMessBill = true;
            List<int> billGeneratedYears = db.MessBills.Where(x => x.month == month).OrderByDescending(x => x.dateOfDeclaration).Select(x => x.dateOfDeclaration).Select(x => x.Year).Distinct().ToList();
            if(billGeneratedYears.Count > 0 && billGeneratedYears.Contains(DateTime.Now.Year))
            {
                canGenerateMessBill = false;
            }
            ViewBag.canGenerateMessBill = canGenerateMessBill;
            return View();
        }

        [HttpPost]
        public ActionResult GenerateMessBill(int dummy = 0)
        {
            List<string> bidList = db.Students.Select(x => x.bid).ToList();
            decimal messFees = db.HostelCharges.Where(x => x.id == 0).OrderByDescending(x => x.year).First().val.Value;
            int month = DateTime.Now.AddMonths(-1).Month;
            int numberOfDays = DateTime.DaysInMonth(DateTime.Now.Year, month);
            foreach (string bid in bidList)
            {
                db.MessBills.Add(new MessBill {
                    bid = bid,
                    dateOfDeclaration = DateTime.Now,
                    month = month,
                    numDays = numberOfDays
                });
                db.SaveChanges();
            }
            return Content("Success!");
        }

        public ActionResult MessTransaction()
        {
            return View();
        }

        public ActionResult PerformMessTransaction(MessTransactionViewModel userInput)
        {
            Student student = db.Students.Where(x => x.bid == userInput.bid).First();
            List<MessFeeDueViewModel> messFeeDues = GetMessDue(userInput.bid).Where(x => x.academicYear == userInput.academicYear && x.month == userInput.month).ToList();
            if(messFeeDues.Count > 0)
            {
                decimal amountPayable = messFeeDues.First().amountDue;
                long billNum = student.MessBills.Where(x => x.month == userInput.month && x.dateOfDeclaration.Year == userInput.year).First().billNum;
                if(userInput.amount > amountPayable)
                {
                    return Content("Can not pay more than bill amount");
                }
                db.MessTransactions.Add(new MessTransaction
                {
                    bid = student.bid,
                    amount = userInput.amount,
                    year = userInput.academicYear,
                    bankName = userInput.bankName,
                    dateOfPay = userInput.dateOfPayment,
                    receipt = userInput.referenceNumber,
                    paymentTypeId = userInput.paymentType,
                    billNum = billNum
                });
                db.SaveChanges();
                return Content("Success");
            }
            return Content("Can not pay fee if not due or if bill not generated");
        }


        [HttpPost]
        public ActionResult MessTransaction(StudentSearchViewModel userInput)
        {
            try
            {
                Student student = db.Students.Where(x => x.bid == userInput.bid).First();
                Allotment allotment = db.Allotments.Where(x => x.bid == userInput.bid).OrderByDescending(x => x.year).First();
                Room room = db.Rooms.Where(x => x.hostelBlockNumber == allotment.hostelBlock && x.roomNumber == allotment.roomNum).First();
                MessTransactionViewModel viewModel = new MessTransactionViewModel()
                {
                    bid = student.bid,
                    name = student.name,
                    dob = student.dob,
                    semester = student.semester,
                    usn = student.usn,
                    gender = db.Genders.Where(x => x.id == student.gender).First().val,
                    course = db.Courses.Where(x => x.id == student.course).First().val,
                    branch = db.Departments.Where(x => x.id == student.branch).First().val,
                    blockNumber = allotment.hostelBlock,
                    roomNumber = allotment.roomNum,
                    roomType = db.RoomTypes.Where(x => x.id == room.roomType).First().val,
                    floorNumber = int.Parse(room.roomNumber.ToString().Substring(0, 1)),
                    doj = allotment.dateOfJoin,
                    year = allotment.year,
                    academicYear = DateTime.Now.Year,
                    dateOfPayment = DateTime.Now.Date,
                    month = DateTime.Now.AddMonths(-1).Month
                };
                List<SelectListItem> paymentTypeList = new List<SelectListItem>();
                foreach (var x in db.PaymentTypes)
                {
                    if (x.id != 4)
                    {
                        paymentTypeList.Add(new SelectListItem { Text = x.val, Value = x.id + "" });
                    }
                }
                ViewBag.paymentTypeList = paymentTypeList;
                List<SelectListItem> academicYearList = new List<SelectListItem>();
                for (int i = allotment.year; i <= DateTime.Now.Year; i++)
                {
                    academicYearList.Add(new SelectListItem { Text = i + "", Value = i + "" });
                }
                ViewBag.academicYearList = new SelectList(academicYearList, "Value", "Text", DateTime.Now.Year + "");
                return PartialView("_AddMessTransaction", viewModel);
            }
            catch (InvalidOperationException)
            {
                return Content("Student not found");
            }
        }


        public ActionResult ReportAbsenceForMess()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ReportAbsenceForMess(MessAbsenceViewModel userInput)
        {
            List<Student> studentList = db.Students.Where(x => x.bid == userInput.bid).ToList();
            if(studentList.Count > 0)
            {
                string bid = studentList.First().bid;
                int month = DateTime.Now.AddMonths(-1).Month;
                int numberOfDays = DateTime.DaysInMonth(DateTime.Now.Year, month);
                MessBill bill = db.MessBills.Where(x => x.bid == userInput.bid && x.month == month).OrderByDescending(x => x.dateOfDeclaration).First();
                if (userInput.numDaysAbsent > bill.numDays)
                {
                    return Content("Number of days absent can not be greater than number of days in month");
                }
                bill.numDays = numberOfDays - userInput.numDaysAbsent;
                db.MessBills.Attach(bill);
                db.Entry(bill).State = EntityState.Modified;
                db.SaveChanges();
                return Content("Success!");
            }
            return Content("Student Not Found");
        }

        [HttpPost]
        public ActionResult ChangeRoom(StudentSearchViewModel userInput)
        {
            List<Student> studentList = db.Students.Where(x => x.bid == userInput.bid).ToList();
            if (studentList.Count > 0)
            {
                Student student = studentList.First();
                Allotment alllotment = student.Allotments.OrderByDescending(x => x.year).First();
                Tuple<List<HostelFeeDueViewModel>, Hashtable> result = GetStudentDues(student.bid);
                List<HostelFeeDueViewModel> viewModel = result.Item1;
                Hashtable totalDues = result.Item2;
                if (viewModel.Where(x => x.academicYear < DateTime.Now.Year).ToList().Count > 0)
                {
                    return Content("Cannot change room, dues pending!");
                }
                List<SelectListItem> blockList = new List<SelectListItem>();
                var blocks = db.Hostels.Where(x => x.occupantType == student.gender).ToList();
                foreach (Hostel hostel in blocks)
                {
                    blockList.Add(new SelectListItem { Text = hostel.blockNumber + "", Value = hostel.blockNumber + "" });
                }
                ViewBag.hostelBlockList = new SelectList(blockList, "Value", "Text", alllotment.hostelBlock + "");
                List<SelectListItem> roomList = new List<SelectListItem>();
                var rooms = db.Rooms.Where(x => x.hostelBlockNumber == alllotment.hostelBlock);
                foreach (Room room in rooms)
                {
                    if (room.roomNumber == alllotment.roomNum || room.currentOccupancy < room.maxOccupancy)
                    {
                        roomList.Add(new SelectListItem { Text = room.roomNumber + "", Value = room.roomNumber + "" });
                    }
                }
                ViewBag.roomNumberList = new SelectList(roomList, "Value", "Text", alllotment.roomNum + "");
                ViewBag.gender = student.Gender1.val;
                TempData["bid"] = student.bid;
                return PartialView("_ChangeRoom");
            }
            return Content("Student not found!");
        }



        public ActionResult ChangeRoomAllotment(ChangeRoomViewModel userInput)
        {
            string bid = (string)TempData.Peek("bid");
            Student student = db.Students.Where(x => x.bid == bid).First();
            Allotment allotment = student.Allotments.OrderByDescending(x => x.year).First();
            Room currentRoom = db.Rooms.Where(x => x.hostelBlockNumber == allotment.hostelBlock && x.roomNumber == allotment.roomNum).First();
            if (allotment.hostelBlock == userInput.hostelBlock && allotment.roomNum == userInput.roomNumber)
            {
                return Content("No change detected!");
            }
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    currentRoom.currentOccupancy = currentRoom.currentOccupancy - 1;
                    db.Rooms.Attach(currentRoom);
                    db.Entry(currentRoom).State = EntityState.Modified;
                    db.SaveChanges();


                    allotment.dateOfLeave = DateTime.Now;
                    db.Allotments.Attach(allotment);
                    db.Entry(allotment).State = EntityState.Modified;
                    db.SaveChanges();


                    db.Allotments.Add(new Allotment()
                    {
                        bid = student.bid,
                        dateOfJoin = DateTime.Now,
                        year = userInput.year,
                        hostelBlock = userInput.hostelBlock,
                        roomNum = userInput.roomNumber,
                    });
                    db.SaveChanges();


                    Room newRoom = db.Rooms.Where(x => x.hostelBlockNumber == userInput.hostelBlock && x.roomNumber == userInput.roomNumber).First();
                    newRoom.currentOccupancy = newRoom.currentOccupancy + 1;
                    db.Rooms.Attach(newRoom);
                    db.Entry(newRoom).State = EntityState.Modified;
                    db.SaveChanges();


                    transaction.Commit();
                }
                catch(Exception)
                {
                    transaction.Rollback();
                }
            }
            return Content("Success!!");
        }

        [HttpPost]
        public ActionResult ViewStudent(StudentSearchViewModel userInput)
        {
            List<Student> studentList = db.Students.Where(x => x.bid == userInput.bid).ToList();
            if (studentList.Count > 0)
            {
                Student student = studentList.First();
                Allotment allotment = student.Allotments.OrderByDescending(x => x.year).First();
                DisplayStudentViewModel viewmodel = new DisplayStudentViewModel()
                {
                    name = student.name,
                    gender = student.Gender1.val,
                    branch = student.Department.val,
                    blockNumber = allotment.hostelBlock,
                    course = student.Course1.val,
                    dob = student.dob,
                    doj = allotment.dateOfJoin,
                    floorNumber = int.Parse(allotment.roomNum.ToString().ElementAt(0) + ""),
                    roomNumber = allotment.roomNum,
                    roomType = allotment.Room.RoomType1.val,
                    semester = student.semester,
                    usn = student.usn,
                    year = allotment.year
                };
                return PartialView("_ViewStudent", viewmodel);
            }
            return Content("Student not found");
        }

        [NonAction]
        public decimal GetFee(int id, int year, string bid, int custom)
        {
            if (custom == 0)
            {
                return db.HostelCharges.Where(x => x.id == id && x.year <= year).OrderByDescending(x => x.year).First().val.Value;
            }
            else
            {
                decimal? amount = db.HostelBills.Where(x => x.bid == bid && x.year == year).Sum(x => x.amount);
                return amount.HasValue ? amount.Value : new decimal(0.0);
            }
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