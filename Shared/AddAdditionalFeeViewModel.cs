using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class AddAdditionalFeeViewModel
    {
        [Required]
        [Display(Name ="Border ID")]
        public string bid { get; set; }

        [Required]
        [Range(1, Double.MaxValue)]
        [Display(Name ="Amount")]
        public decimal amount { get; set; }

        [Required]
        [Range(1000,9999)]
        [Display(Name ="Year")]
        public int year { get; set; }

        [Required]
        [Display(Name ="Description")]
        public string description { get; set; }
    }
}