using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        // Navigation to the schedule
        public virtual ServiceSchedule ServiceSchedule { get; set; }
    }

    public class ServiceSchedule
    {
        public int Id { get; set; }

        public int UserVehicleId { get; set; }
        public virtual UserVehicle UserVehicle { get; set; }

        public string RecurrenceType { get; set; } // Monthly, Quarterly, etc.
        public DateTime NextServiceDate { get; set; }
        public string Notes { get; set; }
    }

}