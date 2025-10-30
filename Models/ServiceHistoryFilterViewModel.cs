using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WestendMotors.Models
{
    public class ServiceHistoryFilterViewModel
    {
        public int VehicleId { get; set; }
        public string VehicleTitle { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Service Type")]
        public string ServiceType { get; set; }

        [Display(Name = "Sort By")]
        public string SortBy { get; set; }

        public List<ServiceRecord> ServiceRecords { get; set; }

        public SelectList ServiceTypes { get; set; }
        public SelectList SortOptions { get; set; }

        public bool IsAdmin { get; set; }
    }
}