using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WestendMotors.Models
{
    public class VehicleImage
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string ImagePath { get; set; }
        public virtual Vehicle Vehicle { get; set; }
    }

}