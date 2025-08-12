using System.ComponentModel.DataAnnotations;

namespace WestendMotors.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; }

        [Required, StringLength(255)]
        public string PasswordHash { get; set; } // Store hashed password

        [Required, StringLength(50)]
        public string Role { get; set; } // "Admin" or "Customer"
    }
}
