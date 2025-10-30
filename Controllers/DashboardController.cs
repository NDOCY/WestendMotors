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
            // Start with available vehicles
            var vehicles = db.Vehicles.Where(v => v.IsAvailable).AsQueryable();

            // Eager load related data to avoid lazy loading issues
            vehicles = vehicles.Include(v => v.Specs).Include(v => v.Images);

            // Filtering
            if (!string.IsNullOrWhiteSpace(search))
            {
                vehicles = vehicles.Where(v =>
                    v.Title.Contains(search) ||
                    (v.Specs != null && (
                        v.Specs.Make.Contains(search) ||
                        v.Specs.Model.Contains(search)
                    ))
                );
            }

            if (!string.IsNullOrWhiteSpace(make) && vehicles.Any(v => v.Specs != null))
                vehicles = vehicles.Where(v => v.Specs.Make == make);

            if (!string.IsNullOrWhiteSpace(model) && vehicles.Any(v => v.Specs != null))
                vehicles = vehicles.Where(v => v.Specs.Model == model);

            if (yearFrom.HasValue && vehicles.Any(v => v.Specs != null))
                vehicles = vehicles.Where(v => v.Specs.Year >= yearFrom.Value);

            if (yearTo.HasValue && vehicles.Any(v => v.Specs != null))
                vehicles = vehicles.Where(v => v.Specs.Year <= yearTo.Value);

            if (minPrice.HasValue)
                vehicles = vehicles.Where(v => v.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                vehicles = vehicles.Where(v => v.Price <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(fuelType) && vehicles.Any(v => v.Specs != null))
                vehicles = vehicles.Where(v => v.Specs.FuelType == fuelType);

            if (!string.IsNullOrWhiteSpace(transmission) && vehicles.Any(v => v.Specs != null))
                vehicles = vehicles.Where(v => v.Specs.Transmission == transmission);

            // Sorting
            switch (sort)
            {
                case "priceLow":
                    vehicles = vehicles.OrderBy(v => v.Price);
                    break;
                case "priceHigh":
                    vehicles = vehicles.OrderByDescending(v => v.Price);
                    break;
                case "newest":
                    vehicles = vehicles.OrderByDescending(v => v.DateAdded);
                    break;
                case "year":
                    vehicles = vehicles.OrderByDescending(v => v.Specs.Year);
                    break;
                default:
                    vehicles = vehicles.OrderByDescending(v => v.DateAdded);
                    break;
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

        // Add this method to get filter options for dropdowns
        [HttpGet]
        public JsonResult GetFilterOptions()
        {
            try
            {
                var makes = db.Vehicles
                    .Where(v => v.IsAvailable && v.Specs != null && v.Specs.Make != null)
                    .Select(v => v.Specs.Make)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                var models = db.Vehicles
                    .Where(v => v.IsAvailable && v.Specs != null && v.Specs.Model != null)
                    .Select(v => v.Specs.Model)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                var fuelTypes = db.Vehicles
                    .Where(v => v.IsAvailable && v.Specs != null && v.Specs.FuelType != null)
                    .Select(v => v.Specs.FuelType)
                    .Distinct()
                    .OrderBy(f => f)
                    .ToList();

                var transmissions = db.Vehicles
                    .Where(v => v.IsAvailable && v.Specs != null && v.Specs.Transmission != null)
                    .Select(v => v.Specs.Transmission)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                var minYear = db.Vehicles
                    .Where(v => v.IsAvailable && v.Specs != null)
                    .Min(v => (int?)v.Specs.Year) ?? DateTime.Now.Year - 20;

                var maxYear = db.Vehicles
                    .Where(v => v.IsAvailable && v.Specs != null)
                    .Max(v => (int?)v.Specs.Year) ?? DateTime.Now.Year;

                var priceRange = db.Vehicles
                    .Where(v => v.IsAvailable)
                    .Select(v => v.Price)
                    .ToList();

                return Json(new
                {
                    makes,
                    models,
                    fuelTypes,
                    transmissions,
                    yearRange = new { min = minYear, max = maxYear },
                    priceRange = new
                    {
                        min = priceRange.Any() ? priceRange.Min() : 0,
                        max = priceRange.Any() ? priceRange.Max() : 100000
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Admin()
        {
            try
            {
                var model = new AdminDashboardViewModel
                {
                    AvailableVehicleCount = db.Vehicles.Count(v => v.IsAvailable),
                    SoldVehicleCount = db.Vehicles.Count(v => !v.IsAvailable),
                    AppointmentCount = db.Appointments.Count(),
                    TradeInRequestCount = db.TradeInRequests.Count(),
                    ServiceRecordCount = db.ServiceRecords.Count(),
                    UserCount = db.Users.Count(u => u.IsActive),

                    UpcomingAppointments = db.Appointments
                        .Include(a => a.Customer)
                        .Include(a => a.Vehicle)
                        .Where(a => a.AppointmentDate >= DateTime.Now && a.Status != "Completed" && a.Status != "Cancelled")
                        .OrderBy(a => a.AppointmentDate)
                        .Take(5)
                        .ToList(),

                    RecentTradeIns = db.TradeInRequests
                        .Include(t => t.Customer)
                        .Include(t => t.TargetVehicle)
                        .OrderByDescending(t => t.RequestDate)
                        .Take(5)
                        .ToList(),

                    RecentServices = db.ServiceRecords
                        .Include(s => s.Vehicle)
                        //.Include(s => s.Customer)
                        .OrderByDescending(s => s.ServiceDate)
                        .Take(5)
                        .ToList(),

                    RecentUsers = db.Users
                        .Where(u => u.IsActive)
                        .OrderByDescending(u => u.UserId)
                        .Take(5)
                        .ToList(),

                    // Status breakdowns with error handling
                    AppointmentStatusCounts = db.Appointments
                        .GroupBy(a => a.Status ?? "Unknown")
                        .ToDictionary(g => g.Key, g => g.Count()),

                    TradeInStatusCounts = db.TradeInRequests
                        .GroupBy(t => t.Status ?? "Pending")
                        .ToDictionary(g => g.Key, g => g.Count()),

                    UserRoleCounts = db.Users
                        .Where(u => u.IsActive)
                        .GroupBy(u => u.Role ?? "Customer")
                        .ToDictionary(g => g.Key, g => g.Count()),

                    // Additional metrics
                    PendingAppointmentsCount = db.Appointments.Count(a => a.Status == "Pending"),
                    PendingTradeInsCount = db.TradeInRequests.Count(t => t.Status == "Pending"),
                    TodayAppointmentsCount = db.Appointments.Count(a => DbFunctions.TruncateTime(a.AppointmentDate) == DateTime.Today)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error loading admin dashboard: {ex.Message}");

                // Return a safe view with minimal data
                return View(new AdminDashboardViewModel
                {
                    AvailableVehicleCount = 0,
                    SoldVehicleCount = 0,
                    AppointmentCount = 0,
                    TradeInRequestCount = 0,
                    ServiceRecordCount = 0,
                    UserCount = 0
                });
            }
        }

        // Add this method for getting dashboard stats (useful for AJAX updates)
        [HttpGet]
        public JsonResult GetDashboardStats()
        {
            try
            {
                var stats = new
                {
                    AvailableVehicles = db.Vehicles.Count(v => v.IsAvailable),
                    SoldVehicles = db.Vehicles.Count(v => !v.IsAvailable),
                    TotalAppointments = db.Appointments.Count(),
                    PendingAppointments = db.Appointments.Count(a => a.Status == "Pending"),
                    TotalTradeIns = db.TradeInRequests.Count(),
                    PendingTradeIns = db.TradeInRequests.Count(t => t.Status == "Pending"),
                    TotalServices = db.ServiceRecords.Count(),
                    TotalUsers = db.Users.Count(u => u.IsActive),
                    TodayAppointments = db.Appointments.Count(a => DbFunctions.TruncateTime(a.AppointmentDate) == DateTime.Today)
                };

                return Json(stats, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}