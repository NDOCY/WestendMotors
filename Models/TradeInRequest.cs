using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WestendMotors.Models
{
    public class TradeInRequest
    {
        public int TradeInRequestId { get; set; }

        // Link to the dealership vehicle
        public int TargetVehicleId { get; set; }
        public virtual Vehicle TargetVehicle { get; set; }

        public int UserId { get; set; }
        public virtual User Customer { get; set; }

        // Customer vehicle details
        [Required] public string Make { get; set; }
        [Required] public string Model { get; set; }
        [Required] public int Year { get; set; }
        [Required] public int Mileage { get; set; }
        public string FuelType { get; set; }    
        public string Transmission { get; set; }
        public string Color { get; set; }
        public double EngineSize { get; set; }
        public int NumberOfSeats { get; set; }
        public string BodyType { get; set; }
        public string ConditionNotes { get; set; }
        public decimal? EstimatedValue { get; set; }

        // ADMIN REVIEW PROPERTIES (ADD THESE)
        public string Status { get; set; } = "Pending"; // Pending, Under Review, Approved, Declined, Scheduled

        [Display(Name = "Admin Notes")]
        [DataType(DataType.MultilineText)]
        public string AdminNotes { get; set; }

        [Display(Name = "Final Offer Amount")]
        [DataType(DataType.Currency)]
        public decimal? FinalOffer { get; set; }

        [Display(Name = "Admin Review Date")]
        public DateTime? AdminReviewDate { get; set; }

        [Display(Name = "Scheduled Appointment")]
        public DateTime? ScheduledAppointment { get; set; }

        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; } = DateTime.Now;
        // ADD THESE STAFF ASSIGNMENT PROPERTIES
        public int? AssignedStaffId { get; set; }

        [ForeignKey("AssignedStaffId")]
        public virtual User AssignedStaff { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Assigned Date")]
        public DateTime? AssignedDate { get; set; }



        // Navigation properties
        public virtual ICollection<TradeInImage> Images { get; set; } = new List<TradeInImage>();
        public virtual ICollection<TradeInAppointment> Appointments { get; set; } = new List<TradeInAppointment>();
    }

    public class TradeInImage
    {
        public int Id { get; set; }
        public int TradeInRequestId { get; set; }
        public string ImagePath { get; set; }
        public virtual TradeInRequest TradeInRequest { get; set; }
    }

    // ADD THIS NEW CLASS FOR APPOINTMENTS
    public class TradeInAppointment
    {
        public int Id { get; set; }

        [Required]
        public int TradeInRequestId { get; set; }

        [Required]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string Notes { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled, No-Show

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Staff assignment (already exists in your model)
        public int? AssignedStaffId { get; set; }

        [ForeignKey("AssignedStaffId")]
        public virtual User AssignedStaff { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Assigned Date")]
        public DateTime? AssignedDate { get; set; }

        // Navigation property
        public virtual TradeInRequest TradeInRequest { get; set; }
    }

         
}