using HostelManagement.Areas.HostelMessManagement.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace BusinessLayer
{
    public class TransactionHelper
    {
        private HostelManagementEntities1 db = new HostelManagementEntities1();

        public List<AcHead> GetAccountHeads()
        {
            return db.AcHeads.ToList();
        }

        public List<PaymentType> GetPaymentTypes(bool includeDiscount)
        {
            if (!includeDiscount)
            {
                return db.PaymentTypes.Where(x => x.id != 4).ToList();
            }
            return db.PaymentTypes.ToList();
        }

        public List<string> GetValidAcademicYears(int yearOfJoining)
        {
            List<string> academicYearList = new List<string>();
            for (int i = yearOfJoining; i <= DateTime.Now.Year; i++)
            {
                academicYearList.Add(i + "");
            }
            return academicYearList;
        }

        public HostelTransactionViewModel ConstructViewModelForHostelTransaction(string bid)
        {
            StudentHelper helper = new StudentHelper();
            // find the student, allotment and room
            Student student = helper.GetStudent(bid);
            if (student != null)
            {
                Allotment allotment = student.Allotments.OrderByDescending(x => x.year).First();
                Room room = db.Rooms.Where(x => x.hostelBlockNumber == allotment.hostelBlock && x.roomNumber == allotment.roomNum).First();

                List<AllotmentDisplayViewModel> allotmentViewModel = helper.ConstructViewModelForAlloment(student);

                // construct the view model
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
                    allotments = allotmentViewModel,
                    academicYear = DateTime.Now.Year,
                    dateOfPayment = DateTime.Now.Date,
                    year = student.Allotments.OrderBy(x => x.year).First().year
                };

                return viewModel;
            }
            return null;
        }

        public HostelTransactionViewModel ConstructViewModelForHostelFeeDiscount(string bid, out string error)
        {
            error = "";
            StudentHelper helper = new StudentHelper();
            Student student = helper.GetStudent(bid);

            // if the student exists
            if (student != null)
            {
                // get the student, his/her allotment and room

                Allotment allotment = student.Allotments.OrderByDescending(x => x.year).First();
                Room room = db.Rooms.Where(x => x.hostelBlockNumber == allotment.hostelBlock && x.roomNumber == allotment.roomNum).First();

                List<AllotmentDisplayViewModel> allotmentViewModel = helper.ConstructViewModelForAlloment(student);

                // construct the hostel transaction view model
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
                    allotments = allotmentViewModel,
                    academicYear = DateTime.Now.Year,
                    dateOfPayment = DateTime.Now.Date,
                    referenceNumber = "DS",
                    bankName = "RNSIT",
                    paymentType = 4,
                    year = student.Allotments.Where(x => x.dateOfLeave == null).First().year
                };

                return viewModel;
            }
            else
            {
                error = "Student not found!";
                return null;
            }
        }

        public string PerformFeeDiscount(HostelTransactionViewModel userInput)
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
            return "Success!!";
        }

        public bool CanGenerateMessBill()
        {
            int month = DateTime.Now.AddMonths(-1).Month;
            bool canGenerateMessBill = true;

            // get all bills generated for the previous month and current year
            List<int> billGeneratedYears = db.MessBills.Where(x => x.month == month).OrderByDescending(x => x.dateOfDeclaration).Select(x => x.dateOfDeclaration).Select(x => x.Year).Distinct().ToList();

            // if bills have alreday been generated, then the user can not generate the bill again
            if (billGeneratedYears.Count > 0 && billGeneratedYears.Contains(DateTime.Now.Year))
            {
                canGenerateMessBill = false;
            }
            return canGenerateMessBill;
        }

        public string GenerateMessBill()
        {
            // get the list of student BIDs and mess fees
            List<Student> studentList = db.Students.ToList();
            decimal messFees = db.HostelCharges.Where(x => x.id == 0).OrderByDescending(x => x.year).First().val.Value;

            // get the previous month and number of days in that month
            int month = DateTime.Now.AddMonths(-1).Month;
            int numberOfDays = DateTime.DaysInMonth(DateTime.Now.AddMonths(-1).Year, month);

            // for each student, generate the buill
            foreach (Student student in studentList)
            {
                Allotment allotment = student.Allotments.Where(x => x.dateOfLeave == null).First();
                if (allotment.dateOfJoin < DateTime.Now.AddMonths(-1))
                {
                    db.MessBills.Add(new MessBill
                    {
                        bid = student.bid,
                        dateOfDeclaration = DateTime.Now,
                        month = month,
                        numDays = numberOfDays,
                        year = GetAcademicYear(DateTime.Now.AddMonths(-1))
                    });
                    db.SaveChanges();
                }
            }
            return "Success!!";
        }

        public string PerformMessTransaction(MessTransactionViewModel userInput)
        {
            // find the student and get his/her mess dues
            Student student = db.Students.Where(x => x.bid == userInput.bid).First();
            List<MessFeeDueViewModel> messFeeDues = GetMessDue(userInput.bid).Where(x => x.academicYear == userInput.academicYear && x.month == userInput.month).ToList();

            // if the student has mess dues
            if (messFeeDues.Count > 0)
            {
                // find the amount payable and the bill number
                decimal amountPayable = messFeeDues.First().amountDue;
                long billNum = student.MessBills.Where(x => x.month == userInput.month && x.year == userInput.academicYear).First().billNum;

                // the user can not pay more than the bill amount
                if (userInput.amount > amountPayable)
                {
                    return "Can not pay more than bill amount";
                }

                // save the transaction to the database
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

                return "Success";
            }

            return "Can not pay fee if not due or if bill not generated";
        }

        public MessTransactionViewModel ConstructViewModelForMessTransaction(string bid, out string error)
        {
            error = "";
            StudentHelper helper = new StudentHelper();

            // find the student, his/her allotmen and room
            Student student = helper.GetStudent(bid);

            if (student != null)
            {
                Allotment allotment = db.Allotments.Where(x => x.bid == bid).OrderByDescending(x => x.year).First();
                Room room = db.Rooms.Where(x => x.hostelBlockNumber == allotment.hostelBlock && x.roomNumber == allotment.roomNum).First();

                List<AllotmentDisplayViewModel> allotmentViewModel = helper.ConstructViewModelForAlloment(student);

                // construct the mess transaction view model
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
                    allotments = allotmentViewModel,
                    academicYear = DateTime.Now.Year,
                    dateOfPayment = DateTime.Now.Date,
                    month = DateTime.Now.AddMonths(-1).Month,
                    year = student.Allotments.Where(x => x.dateOfLeave == null).First().year
                };

                return viewModel;
            }
            else
            {
                error = "Student not found!";
                return null;
            }
        }

        public string ReportAbsenceForMess(MessAbsenceViewModel userInput)
        {
            StudentHelper helper = new StudentHelper();
            // get the student from the database
            Student student = helper.GetStudent(userInput.bid);

            // if the student exists
            if (student != null)
            {
                // get the students BID
                string bid = student.bid;

                // get the previous month and the number of days in that month
                int month = DateTime.Now.AddMonths(-1).Month;
                int numberOfDays = DateTime.DaysInMonth(DateTime.Now.Year, month);

                // get the bill for that month
                List<MessBill> bills = db.MessBills.Where(x => x.bid == userInput.bid && x.month == month).OrderByDescending(x => x.dateOfDeclaration).ToList();

                if(bills.Count <= 0)
                {
                    return "Bill not found!";
                }

                MessBill bill = bills.First();
                // user can not be absent for more number of days than the number of days in the month
                if (userInput.numDaysAbsent > bill.numDays)
                {
                    return "Number of days absent can not be greater than number of days in month";
                }

                // update the absence in the database
                bill.numDays = numberOfDays - userInput.numDaysAbsent;
                db.MessBills.Attach(bill);
                db.Entry(bill).State = EntityState.Modified;
                db.SaveChanges();

                return "Success!";
            }
            return "Student Not Found";
        }

        public string PerformHostelTransaction(HostelTransactionViewModel userInput)
        {
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
            return "Success!";
        }

        /// <summary>
        /// Helper method to get the hostel fee
        /// </summary>
        /// <param name="id">the fee id</param>
        /// <param name="year">the year</param>
        /// <param name="bid">the bid of the student</param>
        /// <param name="custom">wheather the fee is custom or not</param>
        /// <returns></returns>
        public decimal GetFee(int id, int year, string bid, int custom)
        {
            if (id.ToString().EndsWith("5"))
            {
                return 0;
            }
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
        /// Helper method to get a students hostel fee dues
        /// </summary>
        /// <param name="bid">the BID of the student</param>
        /// <param name="allTransactions">true, if we want all transactions, false otherwise</param>
        /// <returns>a list of dues</returns>
        public Tuple<List<HostelFeeDueViewModel>, Hashtable> GetStudentDues(string bid, bool allTransactions = false)
        {
            List<HostelFeeDueViewModel> viewModel = new List<HostelFeeDueViewModel>();
            Hashtable totalDues = new Hashtable();
            Hashtable recurrCost = new Hashtable();

            // retrieve the student from the database and find the most recent year of joining
            Student student = db.Students.Where(x => x.bid == bid).First();
            int yearOfJoining = student.Allotments.OrderBy(x => x.dateOfJoin != null).First().year;

            // for each year since the year of joining until the current year
            for (int i = yearOfJoining; i <= GetAcademicYear(DateTime.Now); i++)
            {
                // find which room the student was in and the type of the room
                Allotment allot = student.Allotments.Where(x => x.year <= i).OrderByDescending(x => x.year).First();
                int roomType = allot.Room.roomType.Value;

                // get the list of charges applicable for the room
                List<AcHead> chargesList = db.AcHeads.ToList();

                // for each charge applicable,
                foreach (AcHead head in chargesList)
                {
                    decimal amount = 0, amountPaid = 0;
                    string customAcHead = "";


                    // get the fee ID
                    int id = int.Parse(student.gender + "" + roomType + "" + head.id);

                    // get the list of transations 
                    List<HostelTransaction> transactions = db.HostelTransactions.Where(x => x.bid == student.bid && x.year == i && x.head == head.id).ToList();

                    // the account head is recurring in nature, then consider the previous payments
                    // if not, do not consider the previous payments
                    if (head.recurr == 1)
                    {
                        // if the current year under consideration is not the students year of joining,
                        // consider the previous payments
                        // if the current year is the students year of joining, then 
                        // do not consider the previous payments
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

                    // find the amount payed 
                    amountPaid = transactions.Sum(x => x.amount).Value;

                    // if the hastbable contains the key, then add the current fee to it,
                    // if not create the key and add
                    if (totalDues.ContainsKey(head.id))
                    {
                        Tuple<string, decimal> temp = ((Tuple<string, decimal>)totalDues[head.id]);
                        totalDues[head.id] = new Tuple<string, decimal>(temp.Item1, temp.Item2 + (amount - amountPaid));
                    }
                    else
                    {
                        totalDues.Add(head.id, new Tuple<string, decimal>(head.val, amount - amountPaid));
                    }

                    // if the fee is custom charge, then construct the ac head accordingly
                    if (head.custom == 1)
                    {
                        // retrieve all the bills for the current student from the database
                        List<HostelBill> bills = student.HostelBills.Where(x => x.year == i).ToList();

                        // for each bill, add the decription to the ac head
                        foreach (HostelBill bill in bills)
                        {
                            customAcHead += bill.descr + ",";
                        }

                        // remove the last ','
                        if (customAcHead.Length > 0)
                        {
                            customAcHead = customAcHead.Substring(0, customAcHead.Length - 1);
                        }
                    }

                    // if all transactions are needed, then add all the values computed above
                    // if not, all the computed values if there is due
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

        /// <summary>
        /// Helper method to get the mess fee due
        /// </summary>
        /// <param name="bid">the students BID</param>
        /// <param name="allTransactions">true, if all transactions are needed, false otherwise</param>
        /// <returns>the mess dues</returns>
        public List<MessFeeDueViewModel> GetMessDue(string bid, bool allTransactions = false)
        {
            List<MessFeeDueViewModel> dues = new List<MessFeeDueViewModel>();

            // find the student and earliest date of joining
            Student student = db.Students.Where(x => x.bid == bid).First();
            DateTime dateOfJoin = student.Allotments.OrderBy(x => x.year).First().dateOfJoin;

            // for each month since the date of joining till now,
            for (DateTime date = dateOfJoin; date <= DateTime.Now; date = date.AddMonths(1))
            {
                int academicYear = GetAcademicYear(date);

                // retrieve all the mess bills
                List<MessBill> bills = db.MessBills.Where(x => x.bid == bid && x.month == date.Month && x.year == academicYear).ToList();

                // if bills exsist for the month under consideration
                if (bills.Count > 0)
                {
                    // retireve the bill, charges per day and amount payed towards this bill
                    MessBill bill = bills.First();
                    decimal messChargesPerDay = db.HostelCharges.Where(x => x.id == 0 && x.year <= academicYear).OrderByDescending(x => x.year).First().val.Value;
                    decimal amountPaid = bill.MessTransactions.Sum(x => x.amount).HasValue ? bill.MessTransactions.Sum(x => x.amount).Value : 0;

                    // if all transactions are needed, then add the values computed above directly,
                    // if not add them only if the there is due
                    if (allTransactions)
                    {
                        dues.Add(new MessFeeDueViewModel
                        {
                            academicYear = academicYear,
                            amount = messChargesPerDay * bill.numDays,
                            amountPaid = amountPaid,
                            amountDue = (messChargesPerDay * bill.numDays) - (amountPaid),
                            month = date.Month,
                            numberOfDays = bill.numDays
                        });
                    }
                    else
                    {
                        if (amountPaid != (messChargesPerDay * bill.numDays))
                        {
                            dues.Add(new MessFeeDueViewModel
                            {
                                academicYear = academicYear,
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

        public decimal GetHostelFeePayable(string head, string bid, int year, bool notDue = true)
        {
            // retrieve the student dues
            Tuple<List<HostelFeeDueViewModel>, Hashtable> result = GetStudentDues(bid, true);
            Hashtable totalDues = result.Item2;

            // extract the hostel fee due from above
            List<HostelFeeDueViewModel> dues = result.Item1;
            if (notDue)
            {
                return dues.Where(x => x.accountHead.StartsWith(head) && x.academicYear == year).Sum(x => x.amountDue);
            }
            return dues.Where(x => x.accountHead.StartsWith(head) && x.academicYear == year).Sum(x => x.amount);
        }

        public decimal GetMessFeePayable(string bid, int month, int year)
        {
            decimal amountDue = 0;

            // get the candidate amounts
            List<decimal> caindateAmountDue = GetMessDue(bid).Where(x => x.academicYear == year && x.month == month).Select(x => x.amountDue).ToList();

            // if a bill has been generated
            if (caindateAmountDue.Count > 0)
            {
                amountDue = caindateAmountDue.First();
            }

            return amountDue;
        }

        public List<StudentDueSummaryViewModel> GetAllHostelDues()
        {
            List<StudentDueSummaryViewModel> viewModel = new List<StudentDueSummaryViewModel>();

            // retrieve the account heads from the database
            List<AcHead> chargesList = db.AcHeads.ToList();

            // retieve the student BID list from the database
            List<string> bidList = db.Students.Select(x => x.bid).ToList();

            // for each student, 
            foreach (string bid in bidList)
            {
                // get the students dues
                Tuple<List<HostelFeeDueViewModel>, Hashtable> result = GetStudentDues(bid);
                Hashtable totalDues = result.Item2;
                bool noDues = true;

                // find if the student has due
                foreach (Tuple<string, decimal> due in totalDues.Values)
                {
                    if (due.Item2 != 0)
                    {
                        noDues = false;
                        break;
                    }
                }

                // if the student has a due, then add the student to the list
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

            return viewModel;
        }

        public List<StudentMessDueSummaryViewModel> GetAllMessDues()
        {
            List<StudentMessDueSummaryViewModel> viewModel = new List<StudentMessDueSummaryViewModel>();

            // retrieve the student BID list from the database
            List<string> bids = db.Students.Select(x => x.bid).ToList();

            // for each student in the database,
            foreach (string bid in bids)
            {
                // get his dues,
                List<MessFeeDueViewModel> dues = GetMessDue(bid);

                // if the student has a due, then add the student to the list
                if (dues.Count > 0)
                {
                    viewModel.Add(new StudentMessDueSummaryViewModel
                    {
                        bid = bid,
                        dues = dues
                    });
                }
            }

            return viewModel;
        }

        public MessChargesViewModel ConstructViewModelForMessFeeChange()
        {
            return new MessChargesViewModel() { dailymess = db.HostelCharges.Where(x => x.id == 0).OrderByDescending(x => x.year).First().val.Value };
        }

        public bool CanChangeMessFees()
        {
            return db.HostelCharges.Where(x => x.id == 0).OrderByDescending(x => x.year).First().year != DateTime.Now.Year;
        }

        public decimal GetFee(int id)
        {
            return db.HostelCharges.Where(x => x.id == id).First().val.Value;
        }

        public int GetRentFeeId(int hostelType, int roomType)
        {
            return int.Parse(hostelType + "" + roomType + "1");
        }

        public int GetFixFeeId(int hostelType, int roomType)
        {
            return int.Parse(hostelType + "" + roomType + "2");
        }

        public int GetDepFeeId(int hostelType, int roomType)
        {
            return int.Parse(hostelType + "" + roomType + "3");
        }

        public bool CanChangeRent(int hostelType, int roomType)
        {
            int temp = GetRentFeeId(hostelType, roomType);
            return db.HostelCharges.Where(x => x.id == temp).OrderByDescending(x => x.year).First().year != DateTime.Now.Year;
        }

        public bool CanChangeFix(int hostelType, int roomType)
        {
            var temp = GetFixFeeId(hostelType, roomType);
            return db.HostelCharges.Where(x => x.id == temp).OrderByDescending(x => x.year).First().year != DateTime.Now.Year;
        }

        public bool CanChangeDep(int hostelType, int roomType)
        {
            var temp = GetDepFeeId(hostelType, roomType);
            return db.HostelCharges.Where(x => x.id == temp).OrderByDescending(x => x.year).First().year != DateTime.Now.Year;
        }

        public string ChangeMessFees(MessChargesViewModel userInput)
        {
            // find the value of the daily mess fees in the database
            decimal originalValue = GetFee(0);

            // if the user has changed the value, update the same in the database
            if (originalValue != userInput.dailymess)
            {
                UpdateFeeInDatabase(0, userInput.dailymess);
                return "Update Success!!";
            }

            return "Update Failed!";
        }

        public HostelChargesViewModel ConstructViewModelForHostelFeeChange(SearchViewModel userInput)
        {
            HostelChargesViewModel model = new HostelChargesViewModel();

            int currentYear = DateTime.Now.Year;

            // get the ID of the various fees
            int rentId = GetRentFeeId(userInput.hostelType, userInput.roomType);
            int fixId = GetFixFeeId(userInput.hostelType, userInput.roomType);
            int depId = GetDepFeeId(userInput.hostelType, userInput.roomType);

            // get the original values of the fees in the database, if not present, zero fill
            try
            {
                model.rent = db.HostelCharges.Where(x => x.id == rentId).OrderByDescending(x => x.year).First().val.Value;
                model.fix = db.HostelCharges.Where(x => x.id == fixId).OrderByDescending(x => x.year).First().val.Value;
                model.deposit = db.HostelCharges.Where(x => x.id == depId).OrderByDescending(x => x.year).First().val.Value;
            }
            catch (InvalidOperationException)
            {
                model.rent = model.fix = model.deposit = 0;
            }

            return model;
        }

        /// <summary>
        /// Helper method to update values in the database
        /// </summary>
        /// <param name="id"> the ID of the value to the change</param>
        /// <param name="changedValue"> the updated value</param>
        public void UpdateFeeInDatabase(int id, decimal changedValue)
        {
            db.HostelCharges.Add(new HostelCharge()
            {
                id = id,
                val = changedValue,
                year = DateTime.Now.Year
            });
            db.SaveChanges();
        }

        public string ChangeHostelFees(HostelChargesViewModel userInput, HostelChargesViewModel originalValues, int rentId, int fixId, int depId)
        {
            // find out the items that the user has changed
            bool rentChanged = originalValues.rent != userInput.rent;
            bool fixChanged = originalValues.fix != userInput.fix;
            bool depChanged = originalValues.deposit != userInput.deposit;

            // if user has changed anything
            if (rentChanged || fixChanged || depChanged)
            {

                // update rent in the database, if changed
                if (rentChanged)
                {
                    UpdateFeeInDatabase(rentId, userInput.rent);
                }

                // update fixed changes in the database, if changed
                if (fixChanged)
                {
                    UpdateFeeInDatabase(fixId, userInput.fix);
                }

                // update deposit in the database, if changed
                if (depChanged)
                {
                    UpdateFeeInDatabase(depId, userInput.deposit);
                }

                // return success message
                return "Update Success!!";
            }

            return "Update Failed!!";
        }

        public List<TransactionsViewModel> GetAllTransactionsForStudent(string bid, bool archieve = false)
        {
            StudentHelper helper = new StudentHelper();

            Student student = helper.GetStudent(bid, archieve);
            List<HostelTransaction> transactions = db.HostelTransactions.Where(x => x.bid == student.bid).ToList();
            List<TransactionsViewModel> viewModel = new List<TransactionsViewModel>();

            foreach (HostelTransaction transcation in transactions)
            {
                if (transcation.amount.Value < 0)
                {
                    viewModel.Add(new TransactionsViewModel
                    {
                        id = transcation.receipt + "",
                        academicYear = transcation.year + " - " + (transcation.year + 1),
                        accountHead = transcation.AcHead.val,
                        amount = -transcation.amount.Value,
                        bankName = transcation.bankName,
                        dateOfPay = transcation.dateOfPay.Date,
                        paymentType = transcation.PaymentType.val,
                        transaction = "Credit"
                    });
                }
                else
                {
                    viewModel.Add(new TransactionsViewModel
                    {
                        id = transcation.receipt + "",
                        academicYear = transcation.year + " - " + (transcation.year + 1),
                        accountHead = transcation.AcHead.val,
                        amount = transcation.amount.Value,
                        bankName = transcation.bankName,
                        dateOfPay = transcation.dateOfPay.Date,
                        paymentType = transcation.PaymentType.val,
                        transaction = "Debit"
                    });
                }
            }

            DateTime dateOfFee = new DateTime(2016, 7, 1);
            Tuple<List<HostelFeeDueViewModel>, Hashtable> result = GetStudentDues(bid, true);
            Hashtable totalDues = result.Item2;
            List<HostelFeeDueViewModel> hostelfees = result.Item1;
            List<Allotment> studentAllotments = student.Allotments.ToList();

            foreach (HostelFeeDueViewModel hostelfee in hostelfees)
            {
                DateTime dateOfBill = new DateTime(hostelfee.academicYear, 7, 1);
                List<Allotment> filtertedStudentAllotments = studentAllotments.Where(x => x.year == hostelfee.academicYear).ToList();
                if (filtertedStudentAllotments.Count > 0)
                {
                    dateOfBill = filtertedStudentAllotments.First().dateOfJoin;
                }
                viewModel.Add(new TransactionsViewModel
                {
                    id = "1",
                    academicYear = hostelfee.academicYear + " - " + (hostelfee.academicYear + 1),
                    accountHead = hostelfee.accountHead,
                    amount = hostelfee.amount,
                    bankName = "Canara Bank",
                    dateOfPay = dateOfBill,
                    paymentType = "",
                    transaction = "Credit"
                });
            }


            List<MessFeeDueViewModel> messFees = GetMessDue(bid, true);
            List<MessBill> messBills = db.MessBills.Where(x => x.bid == bid).ToList();

            foreach (MessFeeDueViewModel messFee in messFees)
            {
                MessBill bill = messBills.Where(x => x.year == messFee.academicYear && x.month == messFee.month).First();
                viewModel.Add(new TransactionsViewModel
                {
                    id = bill.billNum + "",
                    academicYear = messFee.academicYear + " - " + (messFee.academicYear + 1),
                    accountHead = "Mess Bill",
                    amount = messFee.amount,
                    bankName = "Canara Bank",
                    dateOfPay = bill.dateOfDeclaration,
                    paymentType = "...",
                    transaction = "Credit"
                });
            }

            List<MessTransaction> messTransactions = db.MessTransactions.Where(x => x.bid == bid).ToList();
            foreach (MessTransaction transaction in messTransactions)
            {
                viewModel.Add(new TransactionsViewModel
                {
                    academicYear = transaction.year + " - " + (transaction.year + 1),
                    accountHead = "Mess Bill",
                    amount = transaction.amount.Value,
                    bankName = transaction.bankName,
                    paymentType = transaction.PaymentType.val,
                    dateOfPay = transaction.dateOfPay.Date,
                    transaction = "Debit",
                    id = transaction.receipt + ""
                });
            }

            return viewModel.OrderBy(x => x.dateOfPay).ToList();
        }


        public int GetAcademicYear(DateTime date)
        {
            if (date.Month < 7)
            {
                return date.Year - 1;
            }
            return date.Year;
        }

        private TransactionsViewModel GetTransactionViewModel(Student student, int feeId, bool differentFirstBill, DateTime i, string accountHead)
        {
            int academicYear = GetAcademicYear(i);
            return new TransactionsViewModel
            {
                id = "1",
                academicYear = academicYear + " - " + (academicYear + 1),
                accountHead = accountHead,
                amount = GetHostelFeePayable(accountHead, student.bid, academicYear, false),
                bankName = "RNSIT",
                dateOfPay = differentFirstBill ? i.Date : new DateTime(i.Year, 7, 1).Date,
                paymentType = "...",
                transaction = "Credit"
            };
        }
    }
}
