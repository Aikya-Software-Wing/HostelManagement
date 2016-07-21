using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class HostelChargesViewModel
    {
        [Required]
        [Display(Name ="Rent")]
        public decimal rent { get; set; }

        [Required]
        [Display(Name ="Fixed Charges")]
        public decimal fix { get; set; }

        [Required]
        [Display(Name ="Deposit")]
        public decimal deposit { get; set; }
    }
}