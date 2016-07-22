using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class TransactionsViewModel
    {
        public string id { get; set; }
        public DateTime dateOfPay { get; set; }
        public string paymentType { get; set; }
        public string accountHead { get; set; }
        public string bankName { get; set; }
        public string academicYear { get; set; }
        public decimal amount { get; set; }
        public string transaction { get; set; }
    }
}
