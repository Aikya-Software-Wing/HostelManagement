using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HostelManagement.Models
{
    public class LoginModel
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [HiddenInput]
        public string ReturnUrl { get; set; }
    }
}