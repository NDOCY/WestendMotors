using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WestendMotors.Models;

namespace WestendMotors.Controllers
{
    public class VehiclesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Vehicles
        public ActionResult Index()
        {
            try
            {
                // Temporary workaround - load vehicles and specs separately
                var vehicles = db.Vehicles.ToList();

                // Load specs manually to avoid relationship constraint issues
                foreach (var vehicle in vehicles)
                {
                    try
                    {
                        vehicle.Specs = db.VehicleSpec.FirstOrDefault(s => s.VehicleId == vehicle.Id);
                        if (vehicle.Specs == null)
                        {
                            vehicle.Specs = new VehicleSpecs(); // Provide empty specs if none found
                        }
                    }
                    catch
                    {
                        vehicle.Specs = new VehicleSpecs(); // Fallback to empty specs
                    }
                }

                return View(vehicles);
            }
            catch (Exception ex)
            {
                // If all else fails, show vehicles without specs
                var vehicles = db.Vehicles.ToList();
                foreach (var vehicle in vehicles)
                {
                    vehicle.Specs = new VehicleSpecs();
                }
                ViewBag.Error = "There was an issue loading vehicle specifications. Please contact support.";
                return View(vehicles);
            }
        }

        // GET: Vehicles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicle vehicle = db.Vehicles.Find(id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }
            return View(vehicle);
        }

        // GET: Vehicles/Create
        public ActionResult Create()
        {
            var vehicle = new Vehicle
            {
                Specs = new VehicleSpecs()
            };
            return View(vehicle);
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Vehicle vehicle)
        {
            try
            {
                // Debug: Check if ModelState is valid
                if (!ModelState.IsValid)
                {
                    // Log validation errors for debugging
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();

                    // Return the view with validation errors
                    return View(vehicle);
                }

                vehicle.DateAdded = DateTime.Now;
                vehicle.IsAvailable = true;

                // Initialize Images collection if null
                if (vehicle.Images == null)
                {
                    vehicle.Images = new List<VehicleImage>();
                }

                // Handle image uploads BEFORE saving to database
                var uploadedFiles = new List<string>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    if (file != null && file.ContentLength > 0 && !string.IsNullOrEmpty(file.FileName))
                    {
                        var uploadDir = Server.MapPath("~/Uploads/Vehicles");
                        if (!Directory.Exists(uploadDir))
                            Directory.CreateDirectory(uploadDir);

                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);
                        file.SaveAs(filePath);
                        uploadedFiles.Add("/Uploads/Vehicles/" + fileName);
                    }
                }

                // Add vehicle with specs in one transaction
                db.Vehicles.Add(vehicle);

                // Set up the relationship properly
                if (vehicle.Specs != null)
                {
                    vehicle.Specs.Vehicle = vehicle;
                }

                // Add images to the vehicle
                foreach (var imagePath in uploadedFiles)
                {
                    vehicle.Images.Add(new VehicleImage
                    {
                        Vehicle = vehicle,
                        ImagePath = imagePath
                    });
                }

                // Save everything in one transaction
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Get the actual inner exception message
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                var fullMessage = ex.InnerException != null ?
                    $"{ex.Message} Inner Exception: {innerMessage}" :
                    ex.Message;

                ModelState.AddModelError("", "An error occurred while saving the vehicle: " + fullMessage);
                return View(vehicle);
            }
        }

        // GET: Vehicles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicle vehicle = db.Vehicles.Include(v => v.Specs).FirstOrDefault(v => v.Id == id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                db.Entry(vehicle).State = EntityState.Modified;
                if (vehicle.Specs != null)
                {
                    db.Entry(vehicle.Specs).State = EntityState.Modified;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicle vehicle = db.Vehicles.Include(v => v.Specs).FirstOrDefault(v => v.Id == id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vehicle vehicle = db.Vehicles.Include(v => v.Specs).Include(v => v.Images).FirstOrDefault(v => v.Id == id);

            // Delete associated specs
            if (vehicle.Specs != null)
            {
                db.VehicleSpec.Remove(vehicle.Specs);
            }

            // Delete associated images
            if (vehicle.Images != null)
            {
                db.VehicleImages.RemoveRange(vehicle.Images);
            }

            db.Vehicles.Remove(vehicle);
            db.SaveChanges();
            return RedirectToAction("Index");
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