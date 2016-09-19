using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Models
{
    /// <summary>
    /// View model for user register
    /// </summary>
    public class RegisterModel
    {
        /// <summary>
        /// The user ID
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// The unencrypted password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// The role of the user
        /// </summary>
        [Required]
        public string Role { get; set; }

    }
}