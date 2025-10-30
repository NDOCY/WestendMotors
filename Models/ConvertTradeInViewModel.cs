using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;
using WestendMotors.Models;

namespace WestendMotors.Models
{
    public class ConvertTradeInViewModel
    {
        // Trade-in information
        public int TradeInRequestId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int UserId { get; set; }

        // Vehicle details
        [Required] public string Make { get; set; }
        [Required] public string Model { get; set; }
        [Required] public int Year { get; set; }
        [Required] public int Mileage { get; set; }
        public string FuelType { get; set; }
        public string Transmission { get; set; }
        public string Color { get; set; }
        public string BodyType { get; set; }
        public string ConditionNotes { get; set; }
        // Add these missing properties for vehicle specs
        public string EngineSize { get; set; }
        public string FeatureList { get; set; }

        // ADD THESE TWO PROPERTIES:
        [Required]
        [Display(Name = "Vehicle Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public string Status { get; set; }
        public int NumberOfSeats { get; set; }
        public ICollection<TradeInImage> Images { get; set; }

        // Pricing
        [Required] public decimal Price { get; set; }
        public decimal? EstimatedValue { get; set; }
        public decimal? FinalOffer { get; set; }

        // Assignment options
        public bool AssignToCustomer { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; }

        [Display(Name = "Service Recurrence")]
        public string RecurrenceType { get; set; }

        [Display(Name = "Next Service Date")]
        [DataType(DataType.Date)]
        public DateTime NextServiceDate { get; set; }

        [Display(Name = "Service Notes")]
        public string ServiceNotes { get; set; }

        [Display(Name = "Assignment Notes")]
        public string AssignmentNotes { get; set; }

        // For dropdowns
        public SelectList RecurrenceOptions { get; set; }
    }
}