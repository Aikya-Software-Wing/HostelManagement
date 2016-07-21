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
        [Display(Name ="Border ID")]
        public string bid { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name ="Number of Days Absent")]
        public int numDaysAbsent { get; set; }
    }
}