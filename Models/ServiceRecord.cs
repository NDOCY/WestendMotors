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
        public DateTime ServiceDate { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; }

        [DataType(DataType.Currency)]
        public decimal Cost { get; set; }
    }
}
