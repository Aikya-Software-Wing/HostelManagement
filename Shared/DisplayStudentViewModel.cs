using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for display student
    /// </summary>
    public class DisplayStudentViewModel
    {
        /// <summary>
        /// The full name of the student
        /// </summary>

        [Required]
        [Display(Name = "Student Full Name")]
        public string name { get; set; }

        /// <summary>
        /// The university seat number of the student
        /// </summary>
        [Display(Name = "University Seat Number")]
        public string usn { get; set; }

        /// <summary>
        /// The semster to which the student belongs to
        /// </summary>
        [Required]
        [Display(Name = "Semester")]
        public int semester { get; set; }

        /// <summary>
        /// The gender of the stdudent
        /// </summary>
        [Required]
        [Display(Name = "Gender")]
        public string gender { get; set; }

        /// <summary>
        /// The course that the student is taking
        /// </summary>
        [Required]
        [Display(Name = "Course")]
        public string course { get; set; }

        /// <summary>
        /// The department to which the student belongs to
        /// </summary>
        [Required]
        [Display(Name = "Department")]
        public string branch { get; set; }

        /// <summary>
        /// The date of birth of the student
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime dob { get; set; }

        /// <summary>
        /// Academic year
        /// </summary>
        public int year { get; set; }

        [Display(Name = "Aadhar Number")]
        public string aadharNumber { get; set; }

        [Required]
        [Range(1000000000, 9999999999, ErrorMessage = "Enter a valid email address")]
        [Display(Name = "Mobile Number")]
        public decimal phoneNumber { get; set; }

        [Display(Name = "PAN")]
        public string pan { get; set; }

        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Required]
        public string email { get; set; }

        /// <summary>
        /// The rooms that have been alloted to the student so far
        /// </summary>
        public List<AllotmentDisplayViewModel> allotments { get; set; }
    }
}