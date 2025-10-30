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
    public class ServiceRecordsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ServiceRecords - Show all records (Admin view)
        public ActionResult Index()
        {
            var serviceRecords = db.ServiceRecords
                .Include(s => s.Vehicle)
                .OrderByDescending(s => s.ServiceDate)
                .ToList();

            return View(serviceRecords);
        }

        // GET: ServiceRecords/UserHistory - Show service history for logged-in user
        public ActionResult UserHistory()
        {
            var userId = Session["UserId"] as int?;
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Get vehicles assigned to this user
            var userVehicles = db.UserVehicles
                .Where(uv => uv.UserId == userId)
                .Select(uv => uv.VehicleId)
                .ToList();

            var serviceRecords = db.ServiceRecords
                .Include(s => s.Vehicle)
                .Where(s => userVehicles.Contains(s.VehicleId))
                .OrderByDescending(s => s.ServiceDate)
                .ToList();

            ViewBag.IsCustomer = true;
            return View("Index", serviceRecords);
        }

        // GET: ServiceRecords/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ServiceRecord serviceRecord = db.ServiceRecords
                .Include(s => s.Vehicle)
                .FirstOrDefault(s => s.ServiceRecordId == id);

            if (serviceRecord == null)
            {
                return HttpNotFound();
            }

            // Check if user has access to this record
            if (!IsAuthorized(serviceRecord))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(serviceRecord);
        }

        // GET: ServiceRecords/Create
        public ActionResult Create()
        {
            var model = new ServiceRecordViewModel
            {
                ServiceDate = DateTime.Today,
                Vehicles = new SelectList(db.Vehicles, "Id", "Title"),
                ServiceTypes = new SelectList(GetServiceTypes())
            };

            return View(model);
        }

        // POST: ServiceRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ServiceRecordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var serviceRecord = new ServiceRecord
                {
                    VehicleId = viewModel.VehicleId,
                    ServiceDate = viewModel.ServiceDate,
                    Description = viewModel.Description,
                    ServiceType = viewModel.ServiceType,
                    Mileage = viewModel.Mileage,
                    Cost = viewModel.Cost,
                    ServiceCenter = viewModel.ServiceCenter,
                    TechnicianNotes = viewModel.TechnicianNotes,
                    NextServiceDue = viewModel.NextServiceDue,
                    CreatedByUserId = Session["UserId"] as int?,
                    CreatedDate = DateTime.Now
                };

                db.ServiceRecords.Add(serviceRecord);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Service record created successfully!";
                return RedirectToAction("Index");
            }

            // Repopulate dropdowns if validation fails
            viewModel.Vehicles = new SelectList(db.Vehicles, "Id", "Title", viewModel.VehicleId);
            viewModel.ServiceTypes = new SelectList(GetServiceTypes(), viewModel.ServiceType);

            return View(viewModel);
        }

        // GET: ServiceRecords/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ServiceRecord serviceRecord = db.ServiceRecords.Find(id);
            if (serviceRecord == null)
            {
                return HttpNotFound();
            }

            if (!IsAuthorized(serviceRecord, true))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var viewModel = new ServiceRecordViewModel
            {
                ServiceRecordId = serviceRecord.ServiceRecordId,
                VehicleId = serviceRecord.VehicleId,
                ServiceDate = serviceRecord.ServiceDate,
                Description = serviceRecord.Description,
                ServiceType = serviceRecord.ServiceType,
                Mileage = serviceRecord.Mileage,
                Cost = serviceRecord.Cost,
                ServiceCenter = serviceRecord.ServiceCenter,
                TechnicianNotes = serviceRecord.TechnicianNotes,
                NextServiceDue = serviceRecord.NextServiceDue,
                Vehicles = new SelectList(db.Vehicles, "Id", "Title", serviceRecord.VehicleId),
                ServiceTypes = new SelectList(GetServiceTypes(), serviceRecord.ServiceType)
            };

            return View(viewModel);
        }

        // POST: ServiceRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ServiceRecordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var serviceRecord = db.ServiceRecords.Find(viewModel.ServiceRecordId);
                if (serviceRecord == null)
                {
                    return HttpNotFound();
                }

                if (!IsAuthorized(serviceRecord, true))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                serviceRecord.VehicleId = viewModel.VehicleId;
                serviceRecord.ServiceDate = viewModel.ServiceDate;
                serviceRecord.Description = viewModel.Description;
                serviceRecord.ServiceType = viewModel.ServiceType;
                serviceRecord.Mileage = viewModel.Mileage;
                serviceRecord.Cost = viewModel.Cost;
                serviceRecord.ServiceCenter = viewModel.ServiceCenter;
                serviceRecord.TechnicianNotes = viewModel.TechnicianNotes;
                serviceRecord.NextServiceDue = viewModel.NextServiceDue;

                db.Entry(serviceRecord).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Service record updated successfully!";
                return RedirectToAction("Index");
            }

            viewModel.Vehicles = new SelectList(db.Vehicles, "Id", "Title", viewModel.VehicleId);
            viewModel.ServiceTypes = new SelectList(GetServiceTypes(), viewModel.ServiceType);

            return View(viewModel);
        }

        // GET: ServiceRecords/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ServiceRecord serviceRecord = db.ServiceRecords
                .Include(s => s.Vehicle)
                .FirstOrDefault(s => s.ServiceRecordId == id);

            if (serviceRecord == null)
            {
                return HttpNotFound();
            }

            if (!IsAuthorized(serviceRecord, true))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(serviceRecord);
        }

        // POST: ServiceRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ServiceRecord serviceRecord = db.ServiceRecords.Find(id);
            if (serviceRecord == null)
            {
                return HttpNotFound();
            }

            if (!IsAuthorized(serviceRecord, true))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            db.ServiceRecords.Remove(serviceRecord);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Service record deleted successfully!";
            return RedirectToAction("Index");
        }

        // Helper method to check authorization
        private bool IsAuthorized(ServiceRecord serviceRecord, bool adminOnly = false)
        {
            var role = Session["Role"] as string;
            var userId = Session["UserId"] as int?;

            // Admins can do anything
            if (role == "Admin") return true;

            // If adminOnly is required and user is not admin
            if (adminOnly) return false;

            // Customers can only view their own records
            if (role == "Customer" && userId.HasValue)
            {
                var userVehicleIds = db.UserVehicles
                    .Where(uv => uv.UserId == userId)
                    .Select(uv => uv.VehicleId)
                    .ToList();

                return userVehicleIds.Contains(serviceRecord.VehicleId);
            }

            return false;
        }

        // Helper method for service types
        private List<string> GetServiceTypes()
        {
            return new List<string>
            {
                "Oil Change",
                "Tire Rotation",
                "Brake Service",
                "Engine Tune-up",
                "Transmission Service",
                "Coolant Flush",
                "Air Filter Replacement",
                "Battery Replacement",
                "Wheel Alignment",
                "Scheduled Maintenance",
                "Electrical System",
                "Suspension Service",
                "Exhaust System",
                "AC Service",
                "Other"
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // Add these methods to your ServiceRecordsController

        public ActionResult VehicleServiceHistory(int vehicleId, DateTime? startDate, DateTime? endDate,
            string serviceType, string sortBy = "newest")
        {
            var vehicle = db.Vehicles.Find(vehicleId);
            if (vehicle == null)
            {
                return HttpNotFound("Vehicle not found");
            }

            // Check authorization
            var userId = Session["UserId"] as int?;
            var role = Session["Role"] as string;
            var isAdmin = role == "Admin";

            if (!isAdmin)
            {
                // Check if user owns this vehicle
                var userOwnsVehicle = db.UserVehicles.Any(uv => uv.UserId == userId && uv.VehicleId == vehicleId);
                if (!userOwnsVehicle)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
            }

            // Build query
            var query = db.ServiceRecords
                .Include(s => s.Vehicle)
                .Where(s => s.VehicleId == vehicleId);

            // Apply filters
            if (startDate.HasValue)
            {
                query = query.Where(s => s.ServiceDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(s => s.ServiceDate <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(serviceType) && serviceType != "All")
            {
                query = query.Where(s => s.ServiceType == serviceType);
            }

            // Apply sorting
            switch (sortBy)
            {
                case "oldest":
                    query = query.OrderBy(s => s.ServiceDate);
                    break;
                case "costHigh":
                    query = query.OrderByDescending(s => s.Cost);
                    break;
                case "costLow":
                    query = query.OrderBy(s => s.Cost);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(s => s.ServiceDate);
                    break;
            }

            var records = query.ToList();

            var viewModel = new ServiceHistoryFilterViewModel
            {
                VehicleId = vehicleId,
                VehicleTitle = vehicle.Title,
                StartDate = startDate,
                EndDate = endDate,
                ServiceType = serviceType,
                SortBy = sortBy,
                ServiceRecords = records,
                ServiceTypes = new SelectList(GetServiceTypes(), serviceType),
                SortOptions = new SelectList(new[]
                {
            new { Value = "newest", Text = "Newest First" },
            new { Value = "oldest", Text = "Oldest First" },
            new { Value = "costHigh", Text = "Highest Cost" },
            new { Value = "costLow", Text = "Lowest Cost" }
        }, "Value", "Text", sortBy),
                IsAdmin = isAdmin
            };

            return View(viewModel);
        }

        // Quick create method for a specific vehicle
        public ActionResult QuickCreate(int vehicleId)
        {
            var vehicle = db.Vehicles.Find(vehicleId);
            if (vehicle == null)
            {
                return HttpNotFound();
            }

            var viewModel = new ServiceRecordViewModel
            {
                VehicleId = vehicleId,
                ServiceDate = DateTime.Today,
                Vehicles = new SelectList(new[] { vehicle }, "Id", "Title", vehicleId),
                ServiceTypes = new SelectList(GetServiceTypes())
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuickCreate(ServiceRecordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var serviceRecord = new ServiceRecord
                {
                    VehicleId = viewModel.VehicleId,
                    ServiceDate = viewModel.ServiceDate,
                    Description = viewModel.Description,
                    ServiceType = viewModel.ServiceType,
                    Mileage = viewModel.Mileage,
                    Cost = viewModel.Cost,
                    ServiceCenter = viewModel.ServiceCenter,
                    TechnicianNotes = viewModel.TechnicianNotes,
                    NextServiceDue = viewModel.NextServiceDue,
                    CreatedByUserId = Session["UserId"] as int?,
                    CreatedDate = DateTime.Now
                };

                db.ServiceRecords.Add(serviceRecord);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Service record created successfully!";
                return RedirectToAction("VehicleServiceHistory", new { vehicleId = viewModel.VehicleId });
            }

            // Repopulate dropdowns
            var vehicle = db.Vehicles.Find(viewModel.VehicleId);
            viewModel.Vehicles = new SelectList(new[] { vehicle }, "Id", "Title", viewModel.VehicleId);
            viewModel.ServiceTypes = new SelectList(GetServiceTypes(), viewModel.ServiceType);

            return View(viewModel);
        }
    }
}