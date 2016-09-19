using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for removing a student
    /// </summary>
    public class RemoveStudentViewModel : DisplayStudentViewModel
    {
        /// <summary>
        /// The border ID of the student
        /// </summary>
        [Required]
        [Display(Name = "Border ID")]
        public string bid { get; set; }

        /// <summary>
        /// The amount of rent to be refunded
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal rentRefund { get; set; }

        /// <summary>
        /// The amount of fixed deposit to be refunded
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal fixRefund { get; set; }

        /// <summary>
        /// The amount of security deposit to be refunded
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal depRefund { get; set; }

        /// <summary>
        /// The refrence number of the rent refund
        /// </summary>
        public string rentRefundRef { get; set; }

        /// <summary>
        /// The refrence number for the fixed deposit refund
        /// </summary>
        public string fixRefundRef { get; set; }
        
        /// <summary>
        /// The refrence number for the security deposit refund
        /// </summary>
        public string depRefundRef { get; set; }
    }
}
