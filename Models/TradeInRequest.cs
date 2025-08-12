using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public decimal EstimatedValue { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Accepted, Declined, Cancelled, Postponed
        public DateTime? NewAppointmentDate { get; set; }
        public string AdminNotes { get; set; }


        public DateTime RequestDate { get; set; } = DateTime.Now;

        // Uploaded images for customer's vehicle
        public virtual ICollection<TradeInImage> Images { get; set; } = new List<TradeInImage>();
    }

    public class TradeInImage
    {
        public int Id { get; set; }
        public int TradeInRequestId { get; set; }
        public string ImagePath { get; set; }
        public virtual TradeInRequest TradeInRequest { get; set; }
    }
}
