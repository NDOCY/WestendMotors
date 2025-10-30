using System;
using System.ComponentModel.DataAnnotations;

namespace WestendMotors.Models
{
    public class AssignVehicleViewModel
    {
        [Required(ErrorMessage = "User is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Purchase date is required")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }

        [Required(ErrorMessage = "Recurrence type is required")]
        public string RecurrenceType { get; set; }

        [Required(ErrorMessage = "Next service date is required")]
        [DataType(DataType.Date)]
        public DateTime NextServiceDate { get; set; } = DateTime.Today.AddMonths(1);

        [StringLength(500, ErrorMessage = "Service notes cannot exceed 500 characters")]
        public string ServiceNotes { get; set; }
    }
}