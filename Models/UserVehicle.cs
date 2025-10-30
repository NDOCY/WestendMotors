using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace WestendMotors.Models
{
    public class UserVehicle
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int VehicleId { get; set; }
        
        public virtual Vehicle Vehicle { get; set; }

        public DateTime PurchaseDate { get; set; }
        public string Notes { get; set; }

        // Navigation property for service schedules (one-to-many)
        public virtual ICollection<ServiceSchedule> ServiceSchedules { get; set; }

        // Helper property to get the first service schedule for backward compatibility
        [NotMapped]
        public ServiceSchedule FirstServiceSchedule
        {
            get { return ServiceSchedules?.FirstOrDefault(); }
        }

        public UserVehicle()
        {
            ServiceSchedules = new HashSet<ServiceSchedule>();
        }
    }

    public class ServiceSchedule
    {
        public int Id { get; set; }

        [Required]
        public int UserVehicleId { get; set; }
        public virtual UserVehicle UserVehicle { get; set; }

        [Required, StringLength(50)]
        public string RecurrenceType { get; set; } // Monthly, Quarterly, etc.

        [Required]
        public DateTime NextServiceDate { get; set; }

        public string Notes { get; set; }
    }
}