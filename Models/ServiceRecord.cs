using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WestendMotors.Models
{
    public class ServiceRecord
    {
        public int ServiceRecordId { get; set; }

        [Required]
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; }

        [Required]
        [Display(Name = "Service Date")]
        [DataType(DataType.Date)]
        public DateTime ServiceDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Service Type")]
        [StringLength(100)]
        public string ServiceType { get; set; } // e.g., Oil Change, Brake Service, etc.

        [Display(Name = "Mileage")]
        public int? Mileage { get; set; }

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

        // Change from Staff to User
        public int? ServiceTechnicianId { get; set; }

        [ForeignKey("ServiceTechnicianId")]
        public virtual User ServiceTechnician { get; set; }

        // For tracking who created the record
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}