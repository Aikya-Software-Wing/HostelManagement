using System;
using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for mess transactions
    /// </summary>
    public class MessTransactionViewModel : DisplayStudentViewModel
    {
        public decimal id { get; set; }

        /// <summary>
        /// The border ID of the student
        /// </summary>
        [Required]
        [Display(Name = "Border ID")]
        public string bid { get; set; }

        /// <summary>
        /// The date of payment
        /// </summary>
        [Required]
        [Display(Name = "Date of Payment")]
        public DateTime dateOfPayment { get; set; }

        /// <summary>
        /// The payment type, that is, NEFT, challan and so on
        /// </summary>
        [Required]
        [Display(Name = "Payment Type")]
        public int paymentType { get; set; }

        /// <summary>
        /// The reference number for transaction
        /// </summary>
        [Required]
        [Display(Name = "Reference Number")]
        public string referenceNumber { get; set; }

        /// <summary>
        /// The name of the bank
        /// </summary>
        [Required]
        [Display(Name = "Bank Name")]
        public string bankName { get; set; }

        /// <summary>
        /// The academic year
        /// </summary>
        [Required]
        [Range(1000, 9999)]
        [Display(Name = "Academic Year")]
        public int academicYear { get; set; }

        /// <summary>
        /// The month
        /// </summary>
        [Required]
        [Display(Name ="Month")]
        [Range(1, 12)]
        public int month { get; set; }

        /// <summary>
        /// The amount transacted
        /// </summary>
        [Required]
        [Range(1.0, Double.MaxValue)]
        [Display(Name = "Amount")]
        public decimal amount { get; set; }
    }
}