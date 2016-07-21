using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class HostelTransactionViewModel : DisplayStudentViewModel
    {
        [Required]
        [Display(Name ="Border ID")]
        public string bid { get; set; }

        [Required]
        [Display(Name ="Date of Payment")]
        public DateTime dateOfPayment { get; set; } 

        [Required]
        [Display(Name ="Payment Type")]
        public int paymentType { get; set; }

        [Required]
        [Display(Name ="Reference Number")]
        public string referenceNumber { get; set; }

        [Required]
        [Display(Name ="Account Head")]
        public int acHead { get; set; }

        [Required]
        [Display(Name ="Bank Name")]
        public string bankName { get; set; }

        [Required]
        [Range(1000,9999)]
        [Display(Name ="Academic Year")]
        public int academicYear { get; set; }

        [Required]
        [Range(1.0, Double.MaxValue)]
        [Display(Name ="Amount")]
        public decimal amount { get; set; }
    }
}