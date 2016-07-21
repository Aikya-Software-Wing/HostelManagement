using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class ChangeRoomViewModel
    {
        [Required]
        [Display(Name = "Hostel Block")]
        public int hostelBlock { get; set; }

        [Required]
        [Display(Name = "Room Number")]
        public int roomNumber { get; set; }

        [Required]
        [Display(Name = "Room Type")]
        public string roomType { get; set; }

        [Required]
        [Display(Name = "Floor")]
        public string floor { get; set; }

        [Required]
        [Range(1000, 9999)]
        [Display(Name = "Year")]
        public int year { get; set; }
    }
}