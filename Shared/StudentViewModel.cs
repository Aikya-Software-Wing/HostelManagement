using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class StudentViewModel
    {
        [Required]
        [Display(Name ="Student Full Name")]
        public string name { get; set; }

        [Display(Name = "University Seat Number")]
        public string usn { get; set; }

        [Required]
        [Display(Name ="Semester")]
        public int semester { get; set; }

        [Required]
        [Display(Name ="Gender")]
        public int gender { get; set; }

        [Required]
        [Display(Name ="Course")]
        public int course { get; set; }

        [Required]
        [Display(Name ="Department")]
        public int branch { get; set; }

        [Required]
        [Range(2000, 9999)]
        [Display(Name ="Year of Joining")]
        public int year { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name ="Date of Birth")]
        public DateTime dob { get; set; }

        [Required]
        [Display(Name ="Hostel Block Number")]
        public int blockNumber { get; set; }

        [Required]
        [Display(Name ="Room Number")]
        public int roomNumber { get; set; }

        [Required]
        [Display(Name ="Floor Number")]
        public int floorNumber { get; set; }

        [Required]
        [Display(Name ="Room Type")]
        public int roomType { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name ="Date of Joining")]
        public DateTime doj { get; set; }
    }
}