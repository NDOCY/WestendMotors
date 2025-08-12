// Vehicle.cs - Simplified and robust model
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace WestendMotors.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string Status { get; set; }

        public DateTime DateAdded { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation properties
        public virtual VehicleSpecs Specs { get; set; }
        public virtual ICollection<VehicleImage> Images { get; set; }

        public Vehicle()
        {
            Images = new List<VehicleImage>();
            DateAdded = DateTime.Now;
            IsAvailable = true;
        }
    }
}