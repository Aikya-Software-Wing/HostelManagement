using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class DisplayStudentViewModel : StudentViewModel
    {
        [Required]
        [Display(Name = "Gender")]
        public new string gender { get; set; }

        [Required]
        [Display(Name = "Course")]
        public new string course { get; set; }

        [Required]
        [Display(Name = "Department")]
        public new string branch { get; set; }

        [Required]
        [Display(Name = "Room Type")]
        public new string roomType { get; set; }
    }
}