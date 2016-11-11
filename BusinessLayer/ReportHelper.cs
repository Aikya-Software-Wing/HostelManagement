using HostelManagement.Areas.HostelMessManagement.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusinessLayer
{
    /// <summary>
    /// A class with the Business Logic that is used for report generation
    /// </summary>
    public class ReportHelper
    {
        private HostelManagementEntities1 db = new HostelManagementEntities1();

        /// <summary>
        /// Method to extact only valid BID ranges from a set of BID ranges
        /// </summary>
        /// <param name="startBID">an array containing the start values for BID</param>
        /// <param name="endBID">an array containing the end values for BID</param>
        /// <param name="startStudents">an array conataining only valid start values</param>
        /// <param name="endStudents">an anrray containing only valid end values</param>
        public void ExtractValidRanges(string[] startBID, string[] endBID, out List<string> startStudents, out List<string> endStudents)
        {
            StudentHelper helper = new StudentHelper();

            // initialize out parameters
            startStudents = new List<string>();
            endStudents = new List<string>();

            // loop through all the ranges available
            for (int i = 0; i < startBID.Length; i++)
            {
                // retrive the start and end student based on BID from the database
                Student startStudent = helper.GetStudent(startBID[i]);
                Student endStudent = helper.GetStudent(endBID[i]);

                // if the student could not be found in the first attempt, check in the archive
                if(startStudent == null)
                {
                    startStudent = helper.GetStudent(startBID[i], true);
                }
                if(endStudent == null)
                {
                    endStudent = helper.GetStudent(endBID[i], true);
                }

                // check if the start student and end student are present in the database
                if (startStudent != null && endStudent != null)
                {
                    // check if the start student is before or the same as the end student
                    if (int.Parse(startStudent.bid.Substring(4).Trim()) <= int.Parse(endStudent.bid.Substring(4).Trim()))
                    {
                        startStudents.Add(startStudent.bid);
                        endStudents.Add(endStudent.bid);
                    }
                }
            }
        }

        /// <summary>
        /// Method to get all transactions by BID range
        /// </summary>
        /// <param name="startStudents">a list contatining the start student</param>
        /// <param name="endStudents">a list containing the end student</param>
        /// <returns>All transactions made by the students in the BID range</returns>
        public List<TransactionsViewModel> GetTransactionsByStudentRange(List<string> startStudents, List<string> endStudents)
        {
            List<TransactionsViewModel> result = new List<TransactionsViewModel>();
            TransactionHelper helper = new TransactionHelper();

            // iterate over all the ranges
            for (int i = 0; i < startStudents.Count; i++)
            {
                // extact the prefix in the BID, that is, 10cs if the BID is 10cs25
                string currentBidPrefix = startStudents[i].Substring(0, 4);

                // extract the suffix in the BID, that is, 25 if the BID is 10cs25
                int currentBidSuffix = int.Parse(startStudents[i].Substring(4));

                // iterate over all students in the current range
                do
                {
                    var list = helper.GetAllTransactionsForStudent(currentBidPrefix + currentBidSuffix);
                    if(list == null)
                    {
                        list = helper.GetAllTransactionsForStudent(currentBidPrefix + currentBidSuffix, true);
                    }

                    result.AddRange(list);
                    currentBidSuffix++;
                } while (currentBidSuffix <= int.Parse(endStudents[i].Substring(4)));
            }

            return result;
        }

        /// <summary>
        /// Method to get all transactions by date range
        /// </summary>
        /// <param name="startDate">the start date</param>
        /// <param name="endDate">the end date</param>
        /// <returns>a list of all the transactions in the given range</returns>
        public List<TransactionsViewModel> GetTransactionsByDateRange(DateTime startDate, DateTime endDate)
        {
            return GetAllTransactions().Where(x => x.dateOfPay >= startDate && x.dateOfPay <= endDate).ToList();
        }

        /// <summary>
        /// Method to get all transactions by amount range
        /// </summary>
        /// <param name="startAmt">the minimum amount</param>
        /// <param name="endAmt">the maximum amount</param>
        /// <returns>a list of all transactions in the given range</returns>
        public List<TransactionsViewModel> GetTransactionsByAmountRange(decimal startAmt, decimal endAmt)
        {
            return GetAllTransactions().Where(x => x.amount >= startAmt && x.amount <= endAmt).ToList();
        }

        /// <summary>
        /// Method to get all transactions based on payment type
        /// </summary>
        /// <param name="value">the ID corresponding to the payment type</param>
        /// <returns>a list of all the transactions that satisfy the criteria</returns>
        public List<TransactionsViewModel> GetTransactionsByPaymentType(int value)
        {
            TransactionHelper helper = new TransactionHelper();
            string payType = helper.GetPaymentTypes(true).Where(x => x.id == value).Select(x => x.val).First().ToString();
            return GetAllTransactions().Where(x => x.paymentType.Equals(payType)).ToList();
        }

        /// <summary>
        /// Method to get all transactions by the account head
        /// </summary>
        /// <param name="accountHead">the ID corresponding to the account head</param>
        /// <returns>a list of all the transactions that satisfy the criteria</returns>
        public List<TransactionsViewModel> GetTransactionsByAccountHead(int accountHead)
        {
            TransactionHelper helper = new TransactionHelper();
            string acHead = helper.GetAccountHeads().Where(x => x.id == accountHead).Select(x => x.val).First().ToString();
            return GetAllTransactions().Where(x => x.accountHead.Equals(acHead)).ToList();
        }

        /// <summary>
        /// Method to get all transactions by gender
        /// </summary>
        /// <param name="gender">the ID corresponding ot the gender</param>
        /// <returns>a list of all transactions that satisfy the criteria</returns>
        public List<TransactionsViewModel> GetTransactionsByGender(int gender)
        {
            List<string> bidList = db.Students.Where(x => x.gender == gender).Select(x => x.bid).ToList();
            return GetTransactionsByBidList(bidList);
        }

        /// <summary>
        /// Method to get all transactions by semester
        /// </summary>
        /// <param name="sem">the semester</param>
        /// <returns>a list of all the transactions that statisfy the criteria</returns>
        public List<TransactionsViewModel> GetTransactionsBySem(int sem)
        {
            List<string> bidList = db.Students.Where(x => x.semester == sem).Select(x => x.bid).ToList();
            return GetTransactionsByBidList(bidList);
        }

        /// <summary>
        /// Method to get all transactions by course
        /// </summary>
        /// <param name="course">the ID corresponding to the course</param>
        /// <returns>a list of all the transactions that statisfy the criteria</returns>
        public List<TransactionsViewModel> GetTransactionsByCourse(int course)
        {
            List<string> bidList = db.Students.Where(x => x.course == course).Select(x => x.bid).ToList();
            return GetTransactionsByBidList(bidList);
        }

        /// <summary>
        /// Method to get all the transactions
        /// </summary>
        /// <returns>all the transactions</returns>
        public List<TransactionsViewModel> GetAllTransactions()
        {
            List<string> bidList = db.Students.Select(x => x.bid).ToList();
            return GetTransactionsByBidList(bidList);
        }

        /// <summary>
        /// Helper method to get all transactions for a set of students
        /// </summary>
        /// <param name="bidList">a list containing the valid BID of the students</param>
        /// <returns>a list of all the transactions that statisfy the criteria</returns>
        private List<TransactionsViewModel> GetTransactionsByBidList(List<string> bidList)
        {
            List<TransactionsViewModel> result = new List<TransactionsViewModel>();
            TransactionHelper transactionHelper = new TransactionHelper();
            StudentHelper studentHelper = new StudentHelper();

            // for each student in the list
            foreach (string bid in bidList)
            {
                if (studentHelper.GetStudent(bid) != null)
                {
                    // student is not in the archieve
                    result.AddRange(transactionHelper.GetAllTransactionsForStudent(bid));
                }
                else
                {
                    // student is in the archieve
                    result.AddRange(transactionHelper.GetAllTransactionsForStudent(bid, true));
                }
            }

            return result;
        }

        /// <summary>
        /// Method to generate the excel file
        /// </summary>
        /// <param name="viewModel">a list of the transactions</param>
        /// <returns>the generated excel file saved as a memory stream</returns>
        public MemoryStream GenerateExcel(List<TransactionsViewModel> viewModel)
        {
            StudentHelper helper = new StudentHelper();
            int totCols = 0, totRows = 0;

            // open an excel file
            ExcelPackage package = new ExcelPackage();

            // the headers for the table in the excel file
            string[] headers = { "TID", "BID", "Name", "USN", "Gender", "Semester", "Course", "Branch",
                "Date", "Payment Type", "Account Head", "Bank Name", "Academic Year", "Transaction", "Amount" };

            // add a sheet to the excel file
            var sheet = package.Workbook.Worksheets.Add("Report");

            // add the headers to the excel file
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                totCols++;
            }

            // add the data to the excel file
            for (int i = 0; i < viewModel.Count; i++)
            {
                // find the student
                Student student = helper.GetStudent(viewModel[i].bid);
                if (student == null)
                {
                    student = helper.GetStudent(viewModel[i].bid, true);
                }

                // add the data cell by cell
                sheet.Cells[i + 2, 1].Value = viewModel[i].id;
                sheet.Cells[i + 2, 2].Value = student.bid;
                sheet.Cells[i + 2, 3].Value = student.name;
                sheet.Cells[i + 2, 4].Value = student.usn;
                sheet.Cells[i + 2, 5].Value = student.Gender1.val;
                sheet.Cells[i + 2, 6].Value = student.semester;
                sheet.Cells[i + 2, 7].Value = student.Course1.val;
                sheet.Cells[i + 2, 8].Value = student.Department.val;
                sheet.Cells[i + 2, 9].Value = viewModel[i].dateOfPay.ToLongDateString();
                sheet.Cells[i + 2, 10].Value = viewModel[i].paymentType;
                sheet.Cells[i + 2, 11].Value = viewModel[i].accountHead;
                sheet.Cells[i + 2, 12].Value = viewModel[i].bankName;
                sheet.Cells[i + 2, 13].Value = viewModel[i].academicYear;
                sheet.Cells[i + 2, 14].Value = viewModel[i].transaction;
                sheet.Cells[i + 2, 15].Value = viewModel[i].amount;
                totRows++;
            }

            // select the data that has been added to the excel file
            var range = sheet.Cells[1, 1, totRows + 1, totCols];

            // convert the selection to a table
            var table = sheet.Tables.Add(range, "Report");

            // set the table style
            table.TableStyle = OfficeOpenXml.Table.TableStyles.Medium1;
            table.ShowHeader = true;
            table.ShowTotal = true;
            table.Columns[totCols - 1].TotalsRowFunction = OfficeOpenXml.Table.RowFunctions.Sum;

            // initialize a memory stream
            var memStream = new MemoryStream();

            // save the excel file as a memory stream
            package.SaveAs(memStream);

            return memStream;
        }
    }
}
