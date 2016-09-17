using HostelManagement.Areas.HostelMessManagement.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class ReportHelper
    {
        private HostelManagementEntities1 db = new HostelManagementEntities1();

        public void ExtractValidRanges(string[] startBID, string[] endBID, out List<string> startStudents, out List<string> endStudents)
        {
            StudentHelper helper = new StudentHelper();
            startStudents = new List<string>();
            endStudents = new List<string>();
            for (int i = 0; i < startBID.Length; i++)
            {
                Student startStudent = helper.GetStudent(startBID[i]);
                Student endStudent = helper.GetStudent(endBID[i]);
                if (startStudent != null && endStudent != null)
                {
                    if (int.Parse(startStudent.bid.Substring(4).Trim()) < int.Parse(endStudent.bid.Substring(4).Trim()))
                    {
                        startStudents.Add(startStudent.bid);
                        endStudents.Add(endStudent.bid);
                    }
                }
            }
        }

        public List<TransactionsViewModel> GetTransactionsByStudentRange(List<string> startStudents, List<string> endStudents)
        {
            List<TransactionsViewModel> result = new List<TransactionsViewModel>();
            TransactionHelper helper = new TransactionHelper();
            for (int i = 0; i < startStudents.Count; i++)
            {
                string currentBidPrefix = startStudents[i].Substring(0, 4);
                int currentBidSuffix = int.Parse(startStudents[i].Substring(4));
                do
                {
                    result.AddRange(helper.GetAllTransactionsForStudent(currentBidPrefix + currentBidSuffix));
                    currentBidSuffix++;
                } while (currentBidSuffix <= int.Parse(endStudents[i].Substring(4)));
            }
            return result;
        }

        public List<TransactionsViewModel> GetTransactionsByDateRange(DateTime startDate, DateTime endDate)
        {
            return GetAllTransactions().Where(x => x.dateOfPay >= startDate && x.dateOfPay <= endDate).ToList();
        }

        public List<TransactionsViewModel> GetTransactionsByAmountRange(decimal startAmt, decimal endAmt)
        {
            return GetAllTransactions().Where(x => x.amount >= startAmt && x.amount <= endAmt).ToList();
        }

        public List<TransactionsViewModel> GetTransactionsByPaymentType(int value)
        {
            TransactionHelper helper = new TransactionHelper();
            string payType = helper.GetPaymentTypes(true).Where(x => x.id == value).Select(x => x.val).First().ToString();
            return GetAllTransactions().Where(x => x.paymentType.Equals(payType)).ToList();
        }

        public List<TransactionsViewModel> GetTransactionsByAccountHead(int accountHead)
        {
            TransactionHelper helper = new TransactionHelper();
            string payType = helper.GetAccountHeads().Where(x => x.id == accountHead).Select(x => x.val).First().ToString();
            return GetAllTransactions().Where(x => x.accountHead.Equals(payType)).ToList();
        }

        public List<TransactionsViewModel> GetTransactionsByGender(int gender)
        {
            List<string> bidList = db.Students.Where(x => x.gender == gender).Select(x => x.bid).ToList();
            return GetTransactionsByBidList(bidList);
        }

        public List<TransactionsViewModel> GetTransactionsBySem(int sem)
        {
            List<string> bidList = db.Students.Where(x => x.semester == sem).Select(x => x.bid).ToList();
            return GetTransactionsByBidList(bidList);
        }
        public List<TransactionsViewModel> GetTransactionsByCourse(int course)
        {
            List<string> bidList = db.Students.Where(x => x.course == course).Select(x => x.bid).ToList();
            return GetTransactionsByBidList(bidList);
        }

        public List<TransactionsViewModel> GetAllTransactions()
        {
            List<string> bidList = db.Students.Select(x => x.bid).ToList();
            return GetTransactionsByBidList(bidList);
        }

        private List<TransactionsViewModel> GetTransactionsByBidList(List<string> bidList)
        {
            List<TransactionsViewModel> result = new List<TransactionsViewModel>();
            TransactionHelper transactionHelper = new TransactionHelper();
            StudentHelper studentHelper = new StudentHelper();
            foreach (string bid in bidList)
            {
                if (studentHelper.GetStudent(bid) != null)
                {
                    result.AddRange(transactionHelper.GetAllTransactionsForStudent(bid));
                }
                else
                {
                    result.AddRange(transactionHelper.GetAllTransactionsForStudent(bid, true));
                }
            }
            return result;
        }

        public MemoryStream GenerateExcel(List<TransactionsViewModel> viewModel)
        {
            string[] headers = { "TID", "BID", "Name", "USN", "Gender", "Semester", "Course", "Branch",
                "Date", "Payment Type", "Account Head", "Bank Name", "Academic Year", "Transaction", "Amount" };
            StudentHelper helper = new StudentHelper();
            ExcelPackage package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Report");
            int totCols = 0, totRows = 0;
            for(int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                totCols++;
            }
            for(int i = 0; i < viewModel.Count; i++)
            {
                Student student = helper.GetStudent(viewModel[i].bid);
                if(student == null)
                {
                    student = helper.GetStudent(viewModel[i].bid, true);
                }
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
            var range = sheet.Cells[1, 1, totRows + 1, totCols];
            var table = sheet.Tables.Add(range, "Report");
            table.TableStyle = OfficeOpenXml.Table.TableStyles.Medium1;
            table.ShowHeader = true;
            table.ShowTotal = false;
            var memStream = new MemoryStream();
            package.SaveAs(memStream);
            return memStream;
        }
    }
}
