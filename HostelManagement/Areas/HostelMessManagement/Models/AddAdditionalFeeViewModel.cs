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
        public string bid { get; set; }

        [Required]
        [Range(1, Double.MaxValue)]
        public decimal amount { get; set; }

        [Required]
        [Range(1000,9999)]
        public int year { get; set; }

        [Required]
        public string description { get; set; }
    }
}