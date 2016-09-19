using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HostelManagement.Models
{
    /// <summary>
    /// View model for login
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// The user ID
        /// </summary>
        [Required]
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        /// <summary>
        /// The unencrypted password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// The URL that the user should be redirected after successfull login
        /// </summary>
        [HiddenInput]
        public string ReturnUrl { get; set; }
    }
}