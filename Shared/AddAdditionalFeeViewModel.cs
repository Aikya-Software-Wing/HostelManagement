using System;
using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for the add additional fee form
    /// </summary>
    public class AddAdditionalFeeViewModel
    {
        /// <summary>
        /// The Border ID of the student
        /// </summary>
        [Required]
        [Display(Name ="Border ID")]
        public string bid { get; set; }

        /// <summary>
        /// The amount additional fee
        /// </summary>
        [Required]
        [Range(1, Double.MaxValue)]
        [Display(Name ="Amount")]
        public decimal amount { get; set; }

        /// <summary>
        /// The academic year
        /// </summary>
        [Required]
        [Range(1000,9999)]
        [Display(Name ="Year")]
        public int year { get; set; }

        /// <summary>
        /// The description for the additional fee
        /// </summary>
        [Required]
        [Display(Name ="Description")]
        public string description { get; set; }
    }
}