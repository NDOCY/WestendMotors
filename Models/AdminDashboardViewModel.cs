using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WestendMotors.Models
{
    public class AdminDashboardViewModel
    {
        // Summary Counts
        public int AvailableVehicleCount { get; set; }
        public int SoldVehicleCount { get; set; }
        public int AppointmentCount { get; set; }
        public int TradeInRequestCount { get; set; }
        public int ServiceRecordCount { get; set; }
        public int UserCount { get; set; }

        // Additional metrics
        public int PendingAppointmentsCount { get; set; }
        public int PendingTradeInsCount { get; set; }
        public int TodayAppointmentsCount { get; set; }

        // Recent Items
        public List<Appointment> UpcomingAppointments { get; set; }
        public List<TradeInRequest> RecentTradeIns { get; set; }
        public List<ServiceRecord> RecentServices { get; set; }
        public List<User> RecentUsers { get; set; }

        // Status breakdowns
        public Dictionary<string, int> AppointmentStatusCounts { get; set; }
        public Dictionary<string, int> TradeInStatusCounts { get; set; }
        public Dictionary<string, int> UserRoleCounts { get; set; }

        // Performance metrics (optional)
        public decimal MonthlyRevenue { get; set; }
        public int MonthlySales { get; set; }
        public int MonthlyServices { get; set; }

        // Charts data (optional)
        public List<ChartDataPoint> MonthlySalesData { get; set; }
        public List<ChartDataPoint> AppointmentTypeData { get; set; }

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

            MonthlySalesData = new List<ChartDataPoint>();
            AppointmentTypeData = new List<ChartDataPoint>();
        }
    }

    // Helper class for chart data
    public class ChartDataPoint
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
        public int Count { get; set; }

        public ChartDataPoint() { }

        public ChartDataPoint(string label, decimal value, int count = 0)
        {
            Label = label;
            Value = value;
            Count = count;
        }
    }

    // Simplified view model for quick stats
    public class QuickStatsViewModel
    {
        public int AvailableVehicles { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingTradeIns { get; set; }
        public int TodayAppointments { get; set; }
        public int ActiveUsers { get; set; }
        public int CompletedServices { get; set; }
    }
}