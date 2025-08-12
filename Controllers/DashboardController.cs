using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WestendMotors.Models;

namespace WestendMotors.Controllers
{
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index(string search, string make, string model, int? yearFrom, int? yearTo,
                                   decimal? minPrice, decimal? maxPrice, string fuelType, string transmission,
                                   string sort, int page = 1, int pageSize = 9)
        {
            var vehicles = db.Vehicles.Include("Specs").Include("Images").Where(v => v.IsAvailable).AsQueryable();

            // Filtering
            if (!string.IsNullOrWhiteSpace(search))
                vehicles = vehicles.Where(v => v.Title.Contains(search)
                                            || v.Specs.Make.Contains(search)
                                            || v.Specs.Model.Contains(search));

            if (!string.IsNullOrWhiteSpace(make))
                vehicles = vehicles.Where(v => v.Specs.Make == make);

            if (!string.IsNullOrWhiteSpace(model))
                vehicles = vehicles.Where(v => v.Specs.Model == model);

            if (yearFrom.HasValue)
                vehicles = vehicles.Where(v => v.Specs.Year >= yearFrom.Value);

            if (yearTo.HasValue)
                vehicles = vehicles.Where(v => v.Specs.Year <= yearTo.Value);

            if (minPrice.HasValue)
                vehicles = vehicles.Where(v => v.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                vehicles = vehicles.Where(v => v.Price <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(fuelType))
                vehicles = vehicles.Where(v => v.Specs.FuelType == fuelType);

            if (!string.IsNullOrWhiteSpace(transmission))
                vehicles = vehicles.Where(v => v.Specs.Transmission == transmission);

            // Sorting
            switch (sort)
            {
                case "priceLow": vehicles = vehicles.OrderBy(v => v.Price); break;
                case "priceHigh": vehicles = vehicles.OrderByDescending(v => v.Price); break;
                case "newest": vehicles = vehicles.OrderByDescending(v => v.DateAdded); break;
                default: vehicles = vehicles.OrderByDescending(v => v.DateAdded); break;
            }

            // Paging
            var total = vehicles.Count();
            var totalPages = (int)Math.Ceiling((double)total / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var items = vehicles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new DashboardViewModel
            {
                Vehicles = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = totalPages
            };

            // If AJAX, return partial view (grid) only
            if (Request.IsAjaxRequest())
            {
                return PartialView("_VehicleGrid", vm);
            }

            // Else full view
            return View(vm);
        }

        public ActionResult Admin()
        {
            // Summary data
            var availableCount = db.Vehicles.Count(v => v.IsAvailable);
            var soldCount = db.Vehicles.Count(v => !v.IsAvailable);
            var appointmentCount = db.Appointments.Count();
            var upcomingAppointments = db.Appointments
                                         .Where(a => a.AppointmentDate >= DateTime.Now)
                                         .OrderBy(a => a.AppointmentDate)
                                         .Take(5)
                                         .ToList();

            var vm = new AdminDashboardViewModel
            {
                AvailableVehicleCount = availableCount,
                SoldVehicleCount = soldCount,
                AppointmentCount = appointmentCount,
                UpcomingAppointments = upcomingAppointments
            };

            return View(vm);
        }

    }
}
