using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class StudentMessDueSummaryViewModel
    {
        public string bid { get; set; }
        public List<MessFeeDueViewModel> dues { get; set; }
    }
}