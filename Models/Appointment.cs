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

        public string Title { get; set; }

        [Required, StringLength(50)]
        public string AppointmentType { get; set; }

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

        [Display(Name = "Original Appointment")]
        public int? OriginalAppointmentId { get; set; }

        [ForeignKey("OriginalAppointmentId")]
        public virtual Appointment OriginalAppointment { get; set; }
    }
}
