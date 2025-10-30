using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WestendMotors.Models
{
    public class ServiceRecordViewModel
    {
        public int ServiceRecordId { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Service date is required")]
        [Display(Name = "Service Date")]
        [DataType(DataType.Date)]
        public DateTime ServiceDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Display(Name = "Service Type")]
        [StringLength(100)]
        public string ServiceType { get; set; }

        [Display(Name = "Mileage")]
        [Range(0, 1000000, ErrorMessage = "Mileage must be between 0 and 1,000,000")]
        public int? Mileage { get; set; }

        [Required(ErrorMessage = "Cost is required")]
        [Range(0, 10000, ErrorMessage = "Cost must be between 0 and 10,000")]
        [DataType(DataType.Currency)]
        public decimal? Cost { get; set; }

        [Display(Name = "Service Center")]
        [StringLength(200)]
        public string ServiceCenter { get; set; }

        [Display(Name = "Technician Notes")]
        [StringLength(1000)]
        public string TechnicianNotes { get; set; }

        [Display(Name = "Next Service Due")]
        [DataType(DataType.Date)]
        public DateTime? NextServiceDue { get; set; }

        // For dropdown lists
        public SelectList Vehicles { get; set; }
        public SelectList ServiceTypes { get; set; }
    }
}