using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WestendMotors.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User Customer { get; set; }

        public int? VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; }

        [Required, StringLength(50)]
        public string AppointmentType { get; set; }

        // Service-specific fields
        [StringLength(100)]
        public string ServiceType { get; set; } // Oil Change, Brake Service, etc.

        public int? Mileage { get; set; }

        [StringLength(500)]
        public string ServiceDescription { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string Notes { get; set; }

        [StringLength(500)]
        [Display(Name = "Admin Notes")]
        public string AdminNotes { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Rescheduled Date")]
        public DateTime? RescheduledDate { get; set; }

        // Add staff assignment
        public int? AssignedStaffId { get; set; }

        [ForeignKey("AssignedStaffId")]
        public virtual User AssignedStaff { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Assigned Date")]
        public DateTime? AssignedDate { get; set; }

        [Display(Name = "Original Appointment")]
        public int? OriginalAppointmentId { get; set; }

        [ForeignKey("OriginalAppointmentId")]
        public virtual Appointment OriginalAppointment { get; set; }
    }
}
