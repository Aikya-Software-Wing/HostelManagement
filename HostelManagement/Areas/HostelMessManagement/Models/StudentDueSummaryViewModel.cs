using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class StudentDueSummaryViewModel
    {
        public string bid { get; set; }
        public decimal rent { get; set; }
        public decimal fix { get; set; }
        public decimal deposit { get; set; }
        public decimal other { get; set; }
    }
}