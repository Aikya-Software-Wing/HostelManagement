using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for hostel charges
    /// </summary>
    public class HostelChargesViewModel
    {
        /// <summary>
        /// Rent
        /// </summary>
        [Required]
        [Display(Name ="Rent")]
        public decimal rent { get; set; }

        /// <summary>
        /// Fixed charges
        /// </summary>
        [Required]
        [Display(Name ="Fixed Charges")]
        public decimal fix { get; set; }

        /// <summary>
        /// Security deposit
        /// </summary>
        [Required]
        [Display(Name ="Deposit")]
        public decimal deposit { get; set; }
    }
}