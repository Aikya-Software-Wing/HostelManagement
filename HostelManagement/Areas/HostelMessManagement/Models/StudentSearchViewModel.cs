using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class StudentSearchViewModel
    {
        [Required]
        [Display(Name ="Border ID")]
        public string bid { get; set; }
    }
}