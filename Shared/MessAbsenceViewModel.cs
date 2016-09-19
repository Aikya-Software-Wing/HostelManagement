using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for mess absence
    /// </summary>
    public class MessAbsenceViewModel
    {
        /// <summary>
        /// The border ID of the student
        /// </summary>
        [Required]
        [Display(Name ="Border ID")]
        public string bid { get; set; }

        /// <summary>
        /// The number days that the student was absent for the month in question
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name ="Number of Days Absent")]
        public int numDaysAbsent { get; set; }
    }
}