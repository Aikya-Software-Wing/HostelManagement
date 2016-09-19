using System;
using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for allotment display 
    /// </summary>
    public class AllotmentDisplayViewModel
    {
        /// <summary>
        /// The block number for the hostel
        /// </summary>
        [Required]
        [Display(Name = "Hostel Block Number")]
        public int blockNumber { get; set; }

        /// <summary>
        /// The room number
        /// </summary>
        [Required]
        [Display(Name = "Room Number")]
        public int roomNumber { get; set; }

        /// <summary>
        /// The floor number for the room
        /// </summary>
        [Required]
        [Display(Name = "Floor Number")]
        public int floorNumber { get; set; }

        /// <summary>
        /// The type of room
        /// </summary>
        [Required]
        [Display(Name = "Room Type")]
        public string roomType { get; set; }

        /// <summary>
        /// The date of joining the hostel
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Joining")]
        public DateTime doj { get; set; }

        /// <summary>
        /// The date of leaving the hostel
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Leaving")]
        public string dol { get; set; }

        /// <summary>
        /// Academic year during which the student stayed in the hostel
        /// </summary>
        [Required]
        [Display(Name = "Academic Year")]
        public string year { get; set; }
    }
}
