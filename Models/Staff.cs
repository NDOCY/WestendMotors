using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
// Models/Staff.cs
using System.ComponentModel.DataAnnotations;

namespace WestendMotors.Models
{
    public class Staff
    {
        public int StaffId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        [StringLength(50)]
        public string Department { get; set; } // Sales, Service, Admin, etc.

        [StringLength(100)]
        public string Position { get; set; } // Sales Representative, Service Advisor, etc.

        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<TradeInRequest> AssignedTradeIns { get; set; }
    }
}