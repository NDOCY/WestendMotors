using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using WestendMotors.Services;
using System.Net;
using System.Threading.Tasks;
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
        // GET: Vehicles/Create
        public ActionResult Create()
        {
            return View(new Vehicle { Specs = new VehicleSpecs() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Vehicle vehicle, IEnumerable<HttpPostedFileBase> imageFiles)
        {
            System.Diagnostics.Debug.WriteLine($"Files received: {imageFiles?.Count() ?? 0}");

            // Add debug logging for each file
            if (imageFiles != null)
            {
                foreach (var file in imageFiles)
                {
                    System.Diagnostics.Debug.WriteLine($"File: {file?.FileName ?? "null"}, Size: {file?.ContentLength ?? 0}");
                }
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var blobService = new BlobService(ConfigurationManager.AppSettings["AzureBlobConnection"]);

                    vehicle.DateAdded = DateTime.Now;
                    vehicle.IsAvailable = true;
                    vehicle.Images = new List<VehicleImage>();

                    // Initialize Specs if null
                    if (vehicle.Specs == null)
                    {
                        vehicle.Specs = new VehicleSpecs();
                    }

                    // Handle image uploads
                    if (imageFiles != null && imageFiles.Any(f => f != null && f.ContentLength > 0))
                    {
                        System.Diagnostics.Debug.WriteLine($"Processing {imageFiles.Count(f => f != null && f.ContentLength > 0)} valid images");

                        foreach (var file in imageFiles.Where(f => f != null && f.ContentLength > 0))
                        {
                            try
                            {
                                // Validate file type
                                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                                if (!allowedExtensions.Contains(fileExtension))
                                {
                                    ModelState.AddModelError("", $"File {file.FileName} is not a valid image format. Allowed formats: JPG, PNG, GIF");
                                    return View(vehicle);
                                }

                                // Validate file size (e.g., max 5MB)
                                if (file.ContentLength > 5 * 1024 * 1024)
                                {
                                    ModelState.AddModelError("", $"File {file.FileName} is too large. Maximum size is 5MB.");
                                    return View(vehicle);
                                }

                                string imageUrl = await blobService.UploadVehicleImageAsync(file);
                                vehicle.Images.Add(new VehicleImage { ImagePath = imageUrl });
                                System.Diagnostics.Debug.WriteLine($"Successfully uploaded: {file.FileName} -> {imageUrl}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error uploading {file.FileName}: {ex.Message}");
                                ModelState.AddModelError("", $"Error uploading {file.FileName}: {ex.Message}");
                                return View(vehicle);
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No valid image files received");
                    }

                    db.Vehicles.Add(vehicle);
                    await db.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine($"Vehicle created successfully with ID: {vehicle.Id}");
                    return RedirectToAction("Index");
                }
                else
                {
                    // Log validation errors
                    foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        System.Diagnostics.Debug.WriteLine($"Validation Error: {modelError.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in Create method: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            // If we got this far, something failed - return to view
            return View(vehicle);
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Vehicle vehicle = await db.Vehicles
                .Include(v => v.Images)
                .Include(v => v.Specs)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle != null)
            {
                var blobService = new BlobService(ConfigurationManager.AppSettings["AzureBlobConnection"]);

                // Delete all associated images from blob storage
                foreach (var image in vehicle.Images)
                {
                    await blobService.DeleteImageAsync(image.ImagePath);
                }

                // Remove from database
                if (vehicle.Specs != null)
                {
                    db.VehicleSpec.Remove(vehicle.Specs);
                }
                db.Vehicles.Remove(vehicle);
                await db.SaveChangesAsync();
            }

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