using System.Collections.Generic;
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
        public string Role { get; set; } // "Admin", "Sales", "Service", "Customer"

        // Add staff-specific fields
        [Phone, StringLength(20)]
        public string Phone { get; set; }

        [StringLength(100)]
        public string Title { get; set; } // e.g., "Sales Manager", "Service Advisor"

        [StringLength(100)]
        public string Department { get; set; } // e.g., "Sales", "Service", "Administration"

        public bool IsActive { get; set; } = true;

        // Navigation properties - ADD THESE FOR TRADE-IN ASSIGNMENTS
        public virtual ICollection<TradeInRequest> AssignedTradeInRequests { get; set; }
        public virtual ICollection<TradeInAppointment> AssignedTradeInAppointments { get; set; }

        // Navigation properties
        public virtual ICollection<UserVehicle> UserVehicles { get; set; }
        public virtual ICollection<TradeInRequest> AssignedTradeIns { get; set; }
        public virtual ICollection<Appointment> AssignedAppointments { get; set; }
        public virtual ICollection<ServiceRecord> ServiceRecords { get; set; }

        public User()
        {
            UserVehicles = new HashSet<UserVehicle>();
            AssignedTradeIns = new HashSet<TradeInRequest>();
            AssignedAppointments = new HashSet<Appointment>();
            ServiceRecords = new HashSet<ServiceRecord>();
            AssignedTradeInRequests = new HashSet<TradeInRequest>(); // ADD THIS
            AssignedTradeInAppointments = new HashSet<TradeInAppointment>(); // ADD THIS
        }
    }
}