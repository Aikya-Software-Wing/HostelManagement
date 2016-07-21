using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class SearchViewModel
    {
        [Required]
        [Display(Name ="Hostel Type")]
        public int hostelType { get; set; }

        [Required]
        [Display(Name ="Room Type")]
        public int roomType { get; set; }
    }
}