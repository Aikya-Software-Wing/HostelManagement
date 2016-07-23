using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class DisplayStudentViewModel
    {
        [Required]
        [Display(Name = "Student Full Name")]
        public string name { get; set; }

        [Display(Name = "University Seat Number")]
        public string usn { get; set; }

        [Required]
        [Display(Name = "Semester")]
        public int semester { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string gender { get; set; }

        [Required]
        [Display(Name = "Course")]
        public string course { get; set; }

        [Required]
        [Display(Name = "Department")]
        public string branch { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime dob { get; set; }

        public int year { get; set; }

        public List<AllotmentDisplayViewModel> allotments { get; set; }
    }
}