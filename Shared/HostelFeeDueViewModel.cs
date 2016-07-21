using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class HostelFeeDueViewModel
    {
        public int academicYear { get; set; }
        public string accountHead { get; set; }
        public decimal amountPaid { get; set; }
        public decimal amount { get; set; }
        public decimal amountDue { get; set; }
    }
}