using System;
using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Areas.Administration.Models
{
    /// <summary>
    /// View model for add user form
    /// </summary>
    public class AddUserViewModel
    {
        /// <summary>
        /// User name used for login
        /// </summary>
        [Required]
        [Display(Name = "UserName")]
        public string Username { get; set; }

        /// <summary>
        /// Unencrypted password
        /// </summary>
        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// The role of the user
        /// </summary>
        [Required]
        [Display(Name = "Role")]
        public String Role { get; set; }
    }
}