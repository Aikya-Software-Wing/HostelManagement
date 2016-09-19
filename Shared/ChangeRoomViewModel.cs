using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for change room form
    /// </summary>
    public class ChangeRoomViewModel
    {
        /// <summary>
        /// The hostel block number
        /// </summary>
        [Required]
        [Display(Name = "Hostel Block")]
        public int hostelBlock { get; set; }

        /// <summary>
        /// The room number
        /// </summary>
        [Required]
        [Display(Name = "Room Number")]
        public int roomNumber { get; set; }

        /// <summary>
        /// The type of room
        /// </summary>
        [Required]
        [Display(Name = "Room Type")]
        public string roomType { get; set; }

        /// <summary>
        /// The floor number of the room
        /// </summary>
        [Required]
        [Display(Name = "Floor")]
        public string floor { get; set; }

        /// <summary>
        /// The academic year
        /// </summary>
        [Required]
        [Range(1000, 9999)]
        [Display(Name = "Year")]
        public int year { get; set; }
    }
}