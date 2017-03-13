using System;
using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model to display a student
    /// </summary>
    public class StudentViewModel
    {
        /// <summary>
        /// The student full name
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
        /// The semester of the student
        /// </summary>
        [Required]
        [Display(Name = "Semester")]
        public int semester { get; set; }

        /// <summary>
        /// The gender of the student
        /// </summary>
        [Required]
        [Display(Name = "Gender")]
        public int gender { get; set; }

        /// <summary>
        /// The course that the student is taking
        /// </summary>
        [Required]
        [Display(Name = "Course")]
        public int course { get; set; }

        /// <summary>
        /// The department that the student belongs to
        /// </summary>
        [Required]
        [Display(Name = "Department")]
        public int branch { get; set; }

        /// <summary>
        /// The year that the student joined
        /// </summary>
        [Required]
        [Range(2000, 9999)]
        [Display(Name = "Year of Joining")]
        public int year { get; set; }

        /// <summary>
        /// The date of birth of the student
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime dob { get; set; }

        /// <summary>
        /// The hostel block number
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
        /// The floor number
        /// </summary>
        [Required]
        [Display(Name = "Floor Number")]
        public int floorNumber { get; set; }

        /// <summary>
        /// The type of room
        /// </summary>
        [Required]
        [Display(Name = "Room Type")]
        public int roomType { get; set; }

        /// <summary>
        /// The date on which the student joined
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Joining")]
        public DateTime doj { get; set; }

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
    }
}