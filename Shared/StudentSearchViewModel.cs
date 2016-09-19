using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for student search
    /// </summary>
    public class StudentSearchViewModel
    {
        /// <summary>
        /// The border ID of the student
        /// </summary>
        [Required]
        [Display(Name ="Border ID")]
        public string bid { get; set; }
    }
}