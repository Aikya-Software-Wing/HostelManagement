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
        public int hostelBlock { get; set; }

        [Required]
        public int roomNumber { get; set; }

        [Required]
        public string roomType { get; set; }

        [Required]
        public string floor { get; set; }

        [Required]
        [Range(1000, 9999)]
        public int year { get; set; }
    }
}