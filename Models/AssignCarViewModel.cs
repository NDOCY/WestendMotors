using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WestendMotors.Models
{
    public class AssignVehicleViewModel
    {
        public int UserId { get; set; }
        public int VehicleId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Notes { get; set; }

        public string RecurrenceType { get; set; }
        public DateTime NextServiceDate { get; set; }
        public string ServiceNotes { get; set; }
    }


}