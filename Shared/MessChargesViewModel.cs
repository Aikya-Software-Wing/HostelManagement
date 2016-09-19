using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for mess charges
    /// </summary>
    public class MessChargesViewModel
    {
        /// <summary>
        /// Daily mess charges
        /// </summary>
        [Required]
        [Display(Name ="Daily Mess Charges")]
        public decimal dailymess { get; set; }
    }
}