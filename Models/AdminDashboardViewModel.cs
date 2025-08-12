using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WestendMotors.Models
{
    public class AdminDashboardViewModel
    {
        public int AvailableVehicleCount { get; set; }
        public int SoldVehicleCount { get; set; }
        public int AppointmentCount { get; set; }
        public List<Appointment> UpcomingAppointments { get; set; }
    }

}