using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostelManagement.Areas.Administration.Models
{
    public class AddUserViewModel
    {
        [Required]
        [Display(Name = "UserName")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Role")]
        public String Role { get; set; }
    }
}