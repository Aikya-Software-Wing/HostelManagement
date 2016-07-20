using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class MessAbsenceViewModel
    {
        [Required]
        public string bid { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int numDaysAbsent { get; set; }
    }
}