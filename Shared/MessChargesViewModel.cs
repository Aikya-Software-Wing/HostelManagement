using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class MessChargesViewModel
    {
        [Required]
        [Display(Name ="Daily Mess Charges")]
        public decimal dailymess { get; set; }
    }
}