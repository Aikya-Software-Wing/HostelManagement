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
