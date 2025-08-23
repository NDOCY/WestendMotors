using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WestendMotors.Models
{
    public class VehicleImage
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        [Required]

        public string ImagePath { get; set; }
        public virtual Vehicle Vehicle { get; set; }
    }

}