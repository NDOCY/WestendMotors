using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WestendMotors.Models
{
    /*public class AdminDashboardViewModel
    {
        public int AvailableVehicleCount { get; set; }
        public int SoldVehicleCount { get; set; }
        public int AppointmentCount { get; set; }
        public List<Appointment> UpcomingAppointments { get; set; }
    }*/

    public class AdminDashboardViewModel
    {
        // Summary Counts
        public int AvailableVehicleCount { get; set; }
        public int SoldVehicleCount { get; set; }
        public int AppointmentCount { get; set; }
        public int TradeInRequestCount { get; set; }
        public int ServiceRecordCount { get; set; }
        public int UserCount { get; set; }

        // Recent Items
        public List<Appointment> UpcomingAppointments { get; set; }
        public List<TradeInRequest> RecentTradeIns { get; set; }
        public List<ServiceRecord> RecentServices { get; set; }
        public List<User> RecentUsers { get; set; }

        // Optional: Status breakdowns
        public Dictionary<string, int> AppointmentStatusCounts { get; set; }
        public Dictionary<string, int> TradeInStatusCounts { get; set; }
        public Dictionary<string, int> UserRoleCounts { get; set; }

        // Constructor to initialize collections
        public AdminDashboardViewModel()
        {
            UpcomingAppointments = new List<Appointment>();
            RecentTradeIns = new List<TradeInRequest>();
            RecentServices = new List<ServiceRecord>();
            RecentUsers = new List<User>();
            AppointmentStatusCounts = new Dictionary<string, int>();
            TradeInStatusCounts = new Dictionary<string, int>();
            UserRoleCounts = new Dictionary<string, int>();
        }
    }

}