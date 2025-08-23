// VehicleSpecs.cs - Simplified relationship
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WestendMotors.Models
{
    public class VehicleSpecs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = "";

        [Required]
        public int Year { get; set; }

        [Required]
        public int Mileage { get; set; }

        [Required]
        [StringLength(30)]
        public string FuelType { get; set; } = "";

        [Required]
        [StringLength(30)]
        public string Transmission { get; set; } = "";

        [StringLength(30)]
        public string Color { get; set; } = "";

        public double EngineSize { get; set; }

        [Required]
        [Display(Name = "Number of Seats")]
        public int NumberOfSeats { get; set; }

        [Required]
        [Display(Name = "Body Type")]
        [StringLength(30)]
        public string BodyType { get; set; } = "";

        [Display(Name = "Condition Notes")]
        [StringLength(500)]
        public string ConditionNotes { get; set; } = "";

        [Display(Name = "Features")]
        [StringLength(1000)]
        public string FeatureList { get; set; } = "";

        // Navigation property
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; }
    }
}