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
        public string name { get; set; }
        public string usn { get; set; }
        [Required]
        public int semester { get; set; }
        [Required]
        public int gender { get; set; }
        [Required]
        public int course { get; set; }
        [Required]
        public int branch { get; set; }
        [Required]
        [Range(2000, 9999)]
        public int year { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime dob { get; set; }
        [Required]
        public int blockNumber { get; set; }
        [Required]
        public int roomNumber { get; set; }
        [Required]
        public int floorNumber { get; set; }
        [Required]
        public int roomType { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime doj { get; set; }
    }
}