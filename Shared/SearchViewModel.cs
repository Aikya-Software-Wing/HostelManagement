using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model to search for a student
    /// </summary>
    public class SearchViewModel
    {
        /// <summary>
        /// The type of hostel
        /// </summary>
        [Required]
        [Display(Name ="Hostel Type")]
        public int hostelType { get; set; }

        /// <summary>
        /// The type of room
        /// </summary>
        [Required]
        [Display(Name ="Room Type")]
        public int roomType { get; set; }
    }
}