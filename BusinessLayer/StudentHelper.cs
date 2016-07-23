using HostelManagement.Areas.HostelMessManagement.Models;
using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class StudentHelper
    {
        private HostelManagementEntities1 db = new HostelManagementEntities1();

        /// <summary>
        /// Method to add a student to the database
        /// </summary>
        /// <param name="studentDetails">the student details</param>
        /// <returns></returns>
        public Student AddStudent(StudentViewModel studentDetails, out string error)
        {
            error = "";

            // check to see USN is required
            if (IsUsnRequired(studentDetails.semester, studentDetails.usn))
            {
                error = "Please enter the USN";
                return null;
            }

            // generate a unique border ID for the student
            string bid = GenerateBid(studentDetails.branch, studentDetails.year);

            // create the new student
            Student newStudent = new Student()
            {
                bid = bid,
                name = studentDetails.name.Trim(),
                usn = string.IsNullOrEmpty(studentDetails.usn) ? "" : studentDetails.usn,
                semester = studentDetails.semester,
                gender = studentDetails.gender,
                course = studentDetails.course,
                branch = studentDetails.branch,
                dob = studentDetails.dob
            };

            // allot a room to the student
            newStudent.Allotments.Add(new Allotment()
            {
                bid = bid,
                dateOfJoin = studentDetails.doj,
                hostelBlock = studentDetails.blockNumber,
                roomNum = studentDetails.roomNumber,
                year = studentDetails.year
            });

            // update the database
            Room room = db.Rooms.Where(x => x.roomNumber == studentDetails.roomNumber && x.hostelBlockNumber == studentDetails.blockNumber).First();
            ++room.currentOccupancy;
            db.Rooms.Attach(room);
            db.Entry(room).State = EntityState.Modified;
            db.Students.Add(newStudent);
            db.SaveChanges();

            return newStudent;
        }

        /// <summary>
        /// Method to generate a unique border ID 
        /// </summary>
        /// <param name="department"> the department to which the student belongs to </param>
        /// <param name="year"> the year of admission</param>
        /// <returns>the unique border ID</returns>
        private string GenerateBid(int department, int year)
        {
            int numberOfStudent = db.Students.Where(x => x.branch == department && x.bid.ToString().StartsWith(year.ToString().Trim().Substring(2, 2))).ToList().Count + 1;
            return year.ToString().Trim().Substring(2, 2) + db.Departments.Where(x => x.id == department).First().code + numberOfStudent.ToString();
        }

        /// <summary>
        /// Method to get list of genders
        /// </summary>
        /// <returns>list of genders</returns>
        public List<Gender> GetGenders()
        {
            return db.Genders.ToList();
        }

        /// <summary>
        /// Method to get list of courses
        /// </summary>
        /// <returns>list of courses</returns>
        public List<Course> GetCourses()
        {
            return db.Courses.ToList();
        }

        /// <summary>
        /// Method to get list of departments
        /// </summary>
        /// <returns>list of departments</returns>
        public List<Department> GetDepartments()
        {
            return db.Departments.ToList();
        }

        /// <summary>
        /// Method to determine wheater the user should enter a USN
        /// </summary>
        /// <param name="semester">the semester</param>
        /// <param name="usn">the usn</param>
        /// <returns>bool</returns>
        public bool IsUsnRequired(int semester, string usn)
        {
            if (semester > 1 && string.IsNullOrEmpty(usn))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<RoomType> GetRoomTypes()
        {
            return db.RoomTypes.ToList();
        }

        public List<decimal> GetFeeBreakup(string gender, string roomType)
        {
            // get the Database ID for gender and room type
            int gen = int.Parse(gender);
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

            return charges;
        }

        public List<Hostel> GetHostelsForStudent(string gender)
        {
            int genderId = int.Parse(gender);
            List<Hostel> hostels = db.Hostels.Where(x => x.occupantType == genderId).ToList();
            return hostels;
        }

        public List<Room> GetAvailableRoomsInHostel(int hostel)
        {
            List<Room> rooms = db.Rooms.Where(x => x.hostelBlockNumber == hostel && x.currentOccupancy < x.maxOccupancy).ToList();
            return rooms;
        }

        public string GetRoomType(int hostel, int roomNumber)
        {
            return db.Rooms.Where(x => x.roomNumber == roomNumber && x.hostelBlockNumber == hostel).First().RoomType1.val;
        }

        public Student GetStudent(string bid, bool archive = false)
        {
            List<Student> studentList = db.Students.Where(x => x.bid == bid).ToList();
            if (studentList.Count > 0)
            {
                if (archive && !(studentList.First().Allotments.Where(x => x.dateOfLeave == null).Count() > 0))
                {
                    return studentList.First();
                }
                if (!(archive) && studentList.First().Allotments.Where(x => x.dateOfLeave == null).Count() > 0)
                {
                    return studentList.First();
                }
            }
            return null;
        }

        public List<Room> GetRoomsIncludingCurrent(string bid)
        {
            List<Room> roomList = new List<Room>();
            Student student = GetStudent(bid);
            Allotment allotment = student.Allotments.OrderByDescending(x => x.year).First();
            if (student != null)
            {
                var rooms = db.Rooms.Where(x => x.hostelBlockNumber == allotment.hostelBlock);
                foreach (Room room in rooms)
                {
                    if (room.roomNumber == allotment.roomNum || room.currentOccupancy < room.maxOccupancy)
                    {
                        roomList.Add(room);
                    }
                }

                return roomList;
            }

            return null;
        }

        public string PerformRoomChange(ChangeRoomViewModel userInput, string bid)
        {
            TransactionHelper helper = new TransactionHelper();
            Student student = GetStudent(bid);
            Allotment allotment = student.Allotments.OrderByDescending(x => x.year).First();
            Room currentRoom = db.Rooms.Where(x => x.hostelBlockNumber == allotment.hostelBlock && x.roomNumber == allotment.roomNum).First();
            userInput.year = helper.GetAcademicYear(DateTime.Now);

            // if the user did not make any changes
            if (allotment.hostelBlock == userInput.hostelBlock && allotment.roomNum == userInput.roomNumber)
            {
                return "No change detected!";
            }

            if (student.HostelTransactions.Where(x => x.year == userInput.year).ToList().Count > 0)
            {
                return "Hostel Transactions for the year have been performed, can not change room";
            }

            // initiate a transaction
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // reduce the occupancy in the current room
                    currentRoom.currentOccupancy = currentRoom.currentOccupancy - 1;
                    db.Rooms.Attach(currentRoom);
                    db.Entry(currentRoom).State = EntityState.Modified;
                    db.SaveChanges();

                    // add the date of leave
                    allotment.dateOfLeave = DateTime.Now;
                    db.Allotments.Attach(allotment);
                    db.Entry(allotment).State = EntityState.Modified;
                    db.SaveChanges();

                    // add the new allotment
                    db.Allotments.Add(new Allotment()
                    {
                        bid = student.bid,
                        dateOfJoin = DateTime.Now,
                        year = userInput.year,
                        hostelBlock = userInput.hostelBlock,
                        roomNum = userInput.roomNumber,
                    });
                    db.SaveChanges();

                    // change the current occupancy in the new room
                    Room newRoom = db.Rooms.Where(x => x.hostelBlockNumber == userInput.hostelBlock && x.roomNumber == userInput.roomNumber).First();
                    newRoom.currentOccupancy = newRoom.currentOccupancy + 1;
                    db.Rooms.Attach(newRoom);
                    db.Entry(newRoom).State = EntityState.Modified;
                    db.SaveChanges();

                    // commit the transaction
                    transaction.Commit();
                }
                catch (Exception exe)
                {
                    transaction.Rollback();
                    return "An error occured! Please try again later!";
                }
            }
            return "Success!";
        }

        public DisplayStudentViewModel GetStudentDetails(string bid, out string error, bool archive = false)
        {
            error = "";
            // get the student from the database
            Student student = GetStudent(bid, archive);

            // if the student exists
            if (student != null)
            {
                List<AllotmentDisplayViewModel> allotmentViewModel = new List<AllotmentDisplayViewModel>();

                // get the student and the allotment
                foreach (var allotment in student.Allotments)
                {
                    allotmentViewModel.Add(new AllotmentDisplayViewModel
                    {
                        blockNumber = allotment.hostelBlock,
                        doj = allotment.dateOfJoin,
                        dol = allotment.dateOfLeave.HasValue ? allotment.dateOfLeave.Value.Date + "" : "",
                        roomNumber = allotment.roomNum,
                        roomType = allotment.Room.RoomType1.val,
                        floorNumber = allotment.roomNum.ToString().ToCharArray()[0],
                        year = allotment.year + " - " + (allotment.year + 1)
                    });
                }

                // construct the view model
                if (archive)
                {
                    DisplayStudentViewModel viewmodel1 = new DisplayStudentViewModel()
                    {
                        name = student.name,
                        gender = student.Gender1.val,
                        branch = student.Department.val,
                        course = student.Course1.val,
                        dob = student.dob,
                        semester = student.semester,
                        usn = student.usn,
                        allotments = allotmentViewModel
                    };

                    return viewmodel1;
                }
                DisplayStudentViewModel viewmodel = new DisplayStudentViewModel()
                {
                    name = student.name,
                    gender = student.Gender1.val,
                    branch = student.Department.val,
                    course = student.Course1.val,
                    dob = student.dob,
                    semester = student.semester,
                    usn = student.usn,
                    allotments = allotmentViewModel,
                    year = student.Allotments.Where(x => x.dateOfLeave == null).First().year
                };

                return viewmodel;
            }
            else
            {
                error = "Student not found!";
                return null;
            }
        }

        public string AddAdditionalFee(AddAdditionalFeeViewModel userInput)
        {
            Student student = GetStudent(userInput.bid);

            // if student exists
            // if not add error to the model
            if (student != null)
            {
                // find year of joining (most recent)
                int yearOfJoining = student.Allotments.OrderByDescending(x => x.year).First().year;

                // if the user has input a valid year, that is, less then current year and after the student has taken admission
                // if not, add error to the model
                if (userInput.year >= yearOfJoining && userInput.year <= DateTime.Now.Year)
                {
                    // add the bill to the database and save changes
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
                    return "Please enter a valid year";
                }
            }
            else
            {
                return "Student not Found";
            }

            return "Success!";
        }

        public bool CanChangeRoom(string bid, out string error)
        {
            error = "";
            TransactionHelper helper = new TransactionHelper();
            // get the student from the database
            Student student = GetStudent(bid);

            // if the student exists
            if (student != null)
            {
                // find the student his/her allotment and dues
                Allotment alllotment = student.Allotments.Where(x => x.dateOfLeave == null).First();
                int currentAcademicYear = helper.GetAcademicYear(DateTime.Now);

                if (currentAcademicYear == alllotment.year)
                {
                    error = "Can not update allotment more than once per academic year";
                    return false;
                }

                return true;
            }
            error = "Student not found!";
            return false;
        }

        public List<AutoCompleteViewModel> GetStudentListForAutoComplete(string incompleteName)
        {
            List<Student> studentList = db.Students.Where(x => x.name.Contains(incompleteName)).ToList();
            List<AutoCompleteViewModel> list = new List<AutoCompleteViewModel>();
            foreach (Student s in studentList)
            {
                list.Add(new AutoCompleteViewModel
                {
                    label = s.name,
                    value = s.bid,
                    dept = s.Department.code,
                    sem = s.semester + "",
                    gender = s.Gender1.val.ToCharArray()[0] + ""
                });
            }
            return list;
        }

        public bool CanRemoveStudent(string bid, out string error)
        {
            TransactionHelper helper = new TransactionHelper();
            error = "";
            Student student = GetStudent(bid);

            if (student != null)
            {
                List<TransactionsViewModel> transactions = helper.GetAllTransactionsForStudent(bid);

                decimal dues = transactions.Where(x => x.accountHead != "Deposit" && x.transaction == "Credit").Sum(x => x.amount) - transactions.Where(x => x.accountHead != "Deposit" && x.transaction == "Debit").Sum(x => x.amount);
                decimal avaiableDeposit = transactions.Where(x => x.accountHead == "Deposit" && x.transaction == "Debit").Sum(x => x.amount);

                if (dues > avaiableDeposit)
                {
                    error = "Too many dues. Can not remove student!";
                    return false;
                }

                return true;
            }

            error = "Can not find student";
            return false;
        }

        public decimal GetDepositRefund(string bid)
        {
            TransactionHelper helper = new TransactionHelper();
            List<TransactionsViewModel> transactions = helper.GetAllTransactionsForStudent(bid);
            decimal dues = transactions.Where(x => x.accountHead != "Deposit" && x.transaction == "Credit").Sum(x => x.amount) - transactions.Where(x => x.accountHead != "Deposit" && x.transaction == "Debit").Sum(x => x.amount);
            decimal avaiableDeposit = transactions.Where(x => x.accountHead == "Deposit" && x.transaction == "Debit").Sum(x => x.amount);
            return avaiableDeposit - dues;
        }

        public RemoveStudentViewModel ConstructViewModelForRemoveStudent(string bid)
        {
            StudentHelper helper = new StudentHelper();
            // find the student, allotment and room
            Student student = helper.GetStudent(bid);
            if (student != null)
            {
                List<AllotmentDisplayViewModel> allotmentViewModel = ConstructViewModelForAlloment(student);


                // construct the view model
                RemoveStudentViewModel viewModel = new RemoveStudentViewModel()
                {
                    bid = student.bid,
                    name = student.name,
                    dob = student.dob,
                    semester = student.semester,
                    usn = student.usn,
                    gender = db.Genders.Where(x => x.id == student.gender).First().val,
                    course = db.Courses.Where(x => x.id == student.course).First().val,
                    branch = db.Departments.Where(x => x.id == student.branch).First().val,
                    allotments = allotmentViewModel
                };

                return viewModel;
            }
            return null;
        }

        public List<AllotmentDisplayViewModel> ConstructViewModelForAlloment(Student student)
        {
            List<AllotmentDisplayViewModel> allotmentViewModel = new List<AllotmentDisplayViewModel>();

            // get the student and the allotment
            foreach (var allotment in student.Allotments)
            {
                allotmentViewModel.Add(new AllotmentDisplayViewModel
                {
                    blockNumber = allotment.hostelBlock,
                    doj = allotment.dateOfJoin,
                    dol = allotment.dateOfLeave.HasValue ? allotment.dateOfLeave.Value.Date + "" : "",
                    roomNumber = allotment.roomNum,
                    roomType = allotment.Room.RoomType1.val,
                    floorNumber = allotment.roomNum.ToString().ToCharArray()[0],
                    year = allotment.year + " - " + (allotment.year + 1)
                });
            }

            return allotmentViewModel;
        }

        public string PerformRemoveStudent(RemoveStudentViewModel userInput)
        {
            TransactionHelper helper = new TransactionHelper();
            Student student = GetStudent(userInput.bid);
            Allotment allotment = student.Allotments.Where(x => x.dateOfLeave == null).First();
            List<TransactionsViewModel> transactions = helper.GetAllTransactionsForStudent(userInput.bid);
            decimal rentPaid = transactions.Where(x => x.accountHead == "Rent" && x.transaction == "Debit").Sum(x => x.amount);
            decimal fixPaid = transactions.Where(x => x.accountHead == "Fixed Charges" && x.transaction == "Debit").Sum(x => x.amount);
            userInput.depRefund = GetDepositRefund(userInput.bid);

            if (userInput.rentRefund > rentPaid)
            {
                return "Can not refund more rent than paid";
            }

            if (userInput.fixRefund > fixPaid)
            {
                return "Can not refund more fixed changes than paid";
            }

            if (userInput.rentRefund > 0 && string.IsNullOrWhiteSpace(userInput.rentRefundRef))
            {
                return "Reference number needed to refund rent";
            }

            if (userInput.fixRefund > 0 && string.IsNullOrWhiteSpace(userInput.fixRefundRef))
            {
                return "Reference number needed to refund fixed charges";
            }

            if (userInput.depRefund > 0 && string.IsNullOrWhiteSpace(userInput.depRefundRef))
            {
                return "Reference number needed to refund deposit";
            }

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    if (userInput.rentRefund > 0)
                    {
                        db.HostelTransactions.Add(new HostelTransaction
                        {
                            receipt = userInput.rentRefundRef,
                            bankName = "Canara Bank",
                            bid = userInput.bid,
                            amount = -userInput.rentRefund,
                            dateOfPay = DateTime.Now,
                            head = 5,
                            year = helper.GetAcademicYear(DateTime.Now),
                            paymentTypeId = 5
                        });
                        db.SaveChanges();
                    }

                    if (userInput.fixRefund > 0)
                    {
                        db.HostelTransactions.Add(new HostelTransaction
                        {
                            receipt = userInput.fixRefundRef,
                            bankName = "Canara Bank",
                            bid = userInput.bid,
                            amount = -userInput.fixRefund,
                            dateOfPay = DateTime.Now,
                            head = 5,
                            year = helper.GetAcademicYear(DateTime.Now),
                            paymentTypeId = 5
                        });
                        db.SaveChanges();
                    }

                    if (userInput.depRefund > 0)
                    {
                        db.HostelTransactions.Add(new HostelTransaction
                        {
                            receipt = userInput.depRefundRef,
                            bankName = "Canara Bank",
                            bid = userInput.bid,
                            amount = -userInput.depRefund,
                            dateOfPay = DateTime.Now,
                            head = 5,
                            year = helper.GetAcademicYear(DateTime.Now),
                            paymentTypeId = 5
                        });
                        db.SaveChanges();
                    }

                    allotment.dateOfLeave = DateTime.Now;
                    allotment.Room.currentOccupancy -= 1;
                    db.Allotments.Attach(allotment);
                    db.Entry(allotment).State = EntityState.Modified;
                    db.SaveChanges();

                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    return "An error occurred: " + e.Message + "(" + e.InnerException.Message + ")";
                }
            }

            return "Success!";
        }
    }
}
