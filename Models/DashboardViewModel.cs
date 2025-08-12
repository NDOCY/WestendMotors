using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WestendMotors.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<Vehicle> Vehicles { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}