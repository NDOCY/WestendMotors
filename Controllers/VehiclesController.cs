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

        private readonly BlobService _blobService;

        public VehiclesController()
        {
            _blobService = new BlobService();
        }

        // Local image storage path (relative to application)
        private readonly string imageUploadPath = "~/Content/Uploads/VehicleImages/";

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
            return View(new Vehicle { Specs = new VehicleSpecs() });
        }
        /*
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

                        // Ensure upload directory exists
                        string physicalPath = Server.MapPath(imageUploadPath);
                        if (!Directory.Exists(physicalPath))
                        {
                            Directory.CreateDirectory(physicalPath);
                        }

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

                                // Generate unique filename to prevent overwrites
                                string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                                string filePath = Path.Combine(physicalPath, uniqueFileName);

                                // Save file locally
                                file.SaveAs(filePath);

                                // Store relative path in database
                                string relativeImagePath = imageUploadPath.Replace("~", "") + uniqueFileName;
                                vehicle.Images.Add(new VehicleImage { ImagePath = relativeImagePath });

                                System.Diagnostics.Debug.WriteLine($"Successfully saved: {file.FileName} -> {filePath}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error saving {file.FileName}: {ex.Message}");
                                ModelState.AddModelError("", $"Error saving {file.FileName}: {ex.Message}");
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
        }*/
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
    Vehicle vehicle,
    IEnumerable<HttpPostedFileBase> imageFiles,
    int? tradeInRequestId = null) // Make this optional
        {
            System.Diagnostics.Debug.WriteLine($"Files received: {imageFiles?.Count() ?? 0}");
            System.Diagnostics.Debug.WriteLine($"TradeInRequestId: {tradeInRequestId}");

            if (ModelState.IsValid)
            {
                vehicle.DateAdded = DateTime.Now;
                vehicle.IsAvailable = true;
                vehicle.Images = new List<VehicleImage>();

                // Initialize Specs if null
                if (vehicle.Specs == null)
                {
                    vehicle.Specs = new VehicleSpecs();
                }

                // Handle trade-in images if converting from trade-in
                if (tradeInRequestId.HasValue)
                {
                    var tradeInRequest = db.TradeInRequests
                        .Include(t => t.Images)
                        .FirstOrDefault(t => t.TradeInRequestId == tradeInRequestId.Value);

                    if (tradeInRequest != null)
                    {
                        // Copy images from trade-in to vehicle
                        string vehicleImagePath = "~/Content/Uploads/VehicleImages/";
                        string tradeInImagePath = "~/Content/Uploads/TradeInImages/";

                        string physicalVehiclePath = Server.MapPath(vehicleImagePath);
                        string physicalTradeInPath = Server.MapPath(tradeInImagePath);

                        if (!Directory.Exists(physicalVehiclePath))
                        {
                            Directory.CreateDirectory(physicalVehiclePath);
                        }

                        foreach (var tradeInImage in tradeInRequest.Images)
                        {
                            try
                            {
                                string sourceFileName = Path.GetFileName(tradeInImage.ImagePath);
                                string sourcePath = Path.Combine(physicalTradeInPath, sourceFileName);

                                if (System.IO.File.Exists(sourcePath))
                                {
                                    // Generate unique filename for vehicle image
                                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(sourceFileName);
                                    string destPath = Path.Combine(physicalVehiclePath, uniqueFileName);

                                    // Copy the file
                                    System.IO.File.Copy(sourcePath, destPath);

                                    // Create vehicle image record
                                    string relativeImagePath = vehicleImagePath.Replace("~", "") + uniqueFileName;
                                    vehicle.Images.Add(new VehicleImage { ImagePath = relativeImagePath });
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error copying trade-in image: {ex.Message}");
                                // Continue with other images
                            }
                        }

                        // Update trade-in status to "Converted"
                        tradeInRequest.Status = "Converted";
                        tradeInRequest.AdminNotes += $"\nConverted to vehicle listing on {DateTime.Now:yyyy-MM-dd}";
                        db.Entry(tradeInRequest).State = EntityState.Modified;
                    }
                }

                // Handle image uploads (existing code)
                if (imageFiles != null && imageFiles.Any(f => f != null && f.ContentLength > 0))
                {
                    System.Diagnostics.Debug.WriteLine($"Processing {imageFiles.Count(f => f != null && f.ContentLength > 0)} valid images");

                    // Ensure upload directory exists
                    string physicalPath = Server.MapPath(imageUploadPath);
                    if (!Directory.Exists(physicalPath))
                    {
                        Directory.CreateDirectory(physicalPath);
                    }

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

                            // Generate unique filename to prevent overwrites
                            string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                            string filePath = Path.Combine(physicalPath, uniqueFileName);

                            // Save file locally
                            file.SaveAs(filePath);

                            // Store relative path in database
                            string relativeImagePath = imageUploadPath.Replace("~", "") + uniqueFileName;
                            vehicle.Images.Add(new VehicleImage { ImagePath = relativeImagePath });

                            System.Diagnostics.Debug.WriteLine($"Successfully saved: {file.FileName} -> {filePath}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error saving {file.FileName}: {ex.Message}");
                            ModelState.AddModelError("", $"Error saving {file.FileName}: {ex.Message}");
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

                TempData["SuccessMessage"] = tradeInRequestId.HasValue ?
                    "Vehicle created successfully from trade-in!" :
                    "Vehicle created successfully!";

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

            // If we got this far, something failed - return to view
            return View(vehicle);
        }*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            Vehicle vehicle,
            IEnumerable<HttpPostedFileBase> imageFiles,
            int? tradeInRequestId = null)
        {
            System.Diagnostics.Debug.WriteLine($"Files received: {imageFiles?.Count() ?? 0}");
            System.Diagnostics.Debug.WriteLine($"TradeInRequestId: {tradeInRequestId}");

            if (ModelState.IsValid)
            {
                vehicle.DateAdded = DateTime.Now;
                vehicle.IsAvailable = true;
                vehicle.Images = new List<VehicleImage>();

                // Initialize Specs if null
                if (vehicle.Specs == null)
                {
                    vehicle.Specs = new VehicleSpecs();
                }

                // Handle trade-in images if converting from trade-in
                if (tradeInRequestId.HasValue)
                {
                    var tradeInRequest = db.TradeInRequests
                        .Include(t => t.Images)
                        .FirstOrDefault(t => t.TradeInRequestId == tradeInRequestId.Value);

                    if (tradeInRequest != null)
                    {
                        foreach (var tradeInImage in tradeInRequest.Images)
                        {
                            try
                            {
                                // For blob storage, we can reference the same image if it's already in blob storage
                                // Or we can copy it to the vehicles folder
                                if (tradeInImage.ImagePath.StartsWith("http")) // Already in blob storage
                                {
                                    vehicle.Images.Add(new VehicleImage { ImagePath = tradeInImage.ImagePath });
                                }
                                else
                                {
                                    // If it's a local file, upload it to blob storage
                                    // Note: This would require additional logic to handle local files
                                    // For now, we'll just skip or handle accordingly
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error processing trade-in image: {ex.Message}");
                            }
                        }

                        // Update trade-in status to "Converted"
                        tradeInRequest.Status = "Converted";
                        tradeInRequest.AdminNotes += $"\nConverted to vehicle listing on {DateTime.Now:yyyy-MM-dd}";
                        db.Entry(tradeInRequest).State = EntityState.Modified;
                    }
                }

                // Handle new image uploads to Azure Blob Storage
                if (imageFiles != null && imageFiles.Any(f => f != null && f.ContentLength > 0))
                {
                    foreach (var file in imageFiles.Where(f => f != null && f.ContentLength > 0))
                    {
                        try
                        {
                            // Validate file type
                            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                            var fileExtension = Path.GetExtension(file.FileName).ToLower();

                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                ModelState.AddModelError("", $"File {file.FileName} is not a valid image format. Allowed formats: JPG, PNG, GIF, WEBP");
                                return View(vehicle);
                            }

                            // Validate file size (e.g., max 10MB)
                            if (file.ContentLength > 10 * 1024 * 1024)
                            {
                                ModelState.AddModelError("", $"File {file.FileName} is too large. Maximum size is 10MB.");
                                return View(vehicle);
                            }

                            // Upload to Azure Blob Storage
                            string imageUrl = await _blobService.UploadVehicleImageAsync(file);
                            vehicle.Images.Add(new VehicleImage { ImagePath = imageUrl });

                            System.Diagnostics.Debug.WriteLine($"Successfully uploaded to blob storage: {file.FileName} -> {imageUrl}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error uploading {file.FileName}: {ex.Message}");
                            ModelState.AddModelError("", $"Error uploading {file.FileName}: {ex.Message}");
                            return View(vehicle);
                        }
                    }
                }

                db.Vehicles.Add(vehicle);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = tradeInRequestId.HasValue ?
                    "Vehicle created successfully from trade-in!" :
                    "Vehicle created successfully!";

                return RedirectToAction("Index");
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
                // Delete all associated images from Azure Blob Storage
                foreach (var image in vehicle.Images)
                {
                    try
                    {
                        if (image.ImagePath.StartsWith("http")) // Only delete blob storage images
                        {
                            await _blobService.DeleteImageAsync(image.ImagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error deleting image {image.ImagePath}: {ex.Message}");
                        // Continue with deletion even if image deletion fails
                    }
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
        /*
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
                // Delete all associated images from local storage
                string physicalPath = Server.MapPath(imageUploadPath);
                foreach (var image in vehicle.Images)
                {
                    try
                    {
                        string imageName = Path.GetFileName(image.ImagePath);
                        string fullImagePath = Path.Combine(physicalPath, imageName);
                        if (System.IO.File.Exists(fullImagePath))
                        {
                            System.IO.File.Delete(fullImagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error deleting image {image.ImagePath}: {ex.Message}");
                        // Continue with deletion even if image deletion fails
                    }
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
        }*/

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Vehicles/CreateFromTradeIn
        [Authorize(Roles = "Admin")]
        public ActionResult CreateFromTradeIn(Vehicle vehicle)
        {
            // Get trade-in images from TempData
            var tradeInImages = TempData["TradeInImages"] as List<TradeInImage>;
            ViewBag.TradeInImages = tradeInImages;
            ViewBag.TradeInRequestId = TempData["TradeInRequestId"];

            if (vehicle == null)
            {
                vehicle = new Vehicle { Specs = new VehicleSpecs() };
            }

            return View("Create", vehicle);
        }
        /*
        // POST: Vehicles/Create (modified to handle trade-in conversion)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Vehicle vehicle, IEnumerable<HttpPostedFileBase> imageFiles, int? tradeInRequestId = null)
        {
            if (ModelState.IsValid)
            {
                vehicle.DateAdded = DateTime.Now;
                vehicle.IsAvailable = true;
                vehicle.Images = new List<VehicleImage>();

                // Initialize Specs if null
                if (vehicle.Specs == null)
                {
                    vehicle.Specs = new VehicleSpecs();
                }

                // Handle trade-in images if converting from trade-in
                if (tradeInRequestId.HasValue)
                {
                    var tradeInRequest = db.TradeInRequests
                        .Include(t => t.Images)
                        .FirstOrDefault(t => t.TradeInRequestId == tradeInRequestId.Value);

                    if (tradeInRequest != null)
                    {
                        // Copy images from trade-in to vehicle
                        string vehicleImagePath = "~/Content/Uploads/VehicleImages/";
                        string tradeInImagePath = "~/Content/Uploads/TradeInImages/";

                        string physicalVehiclePath = Server.MapPath(vehicleImagePath);
                        string physicalTradeInPath = Server.MapPath(tradeInImagePath);

                        if (!Directory.Exists(physicalVehiclePath))
                        {
                            Directory.CreateDirectory(physicalVehiclePath);
                        }

                        foreach (var tradeInImage in tradeInRequest.Images)
                        {
                            try
                            {
                                string sourceFileName = Path.GetFileName(tradeInImage.ImagePath);
                                string sourcePath = Path.Combine(physicalTradeInPath, sourceFileName);

                                if (System.IO.File.Exists(sourcePath))
                                {
                                    // Generate unique filename for vehicle image
                                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(sourceFileName);
                                    string destPath = Path.Combine(physicalVehiclePath, uniqueFileName);

                                    // Copy the file
                                    System.IO.File.Copy(sourcePath, destPath);

                                    // Create vehicle image record
                                    string relativeImagePath = vehicleImagePath.Replace("~", "") + uniqueFileName;
                                    vehicle.Images.Add(new VehicleImage { ImagePath = relativeImagePath });
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error copying trade-in image: {ex.Message}");
                                // Continue with other images
                            }
                        }

                        // Update trade-in status to "Converted"
                        tradeInRequest.Status = "Converted";
                        tradeInRequest.AdminNotes += $"\nConverted to vehicle listing on {DateTime.Now:yyyy-MM-dd}";
                        db.Entry(tradeInRequest).State = EntityState.Modified;
                    }
                }

                // Handle new image uploads (existing code)
                if (imageFiles != null && imageFiles.Any(f => f != null && f.ContentLength > 0))
                {
                    // ... your existing image upload code ...
                }

                db.Vehicles.Add(vehicle);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = tradeInRequestId.HasValue ?
                    "Vehicle created successfully from trade-in!" :
                    "Vehicle created successfully!";

                return RedirectToAction("Index");
            }

            return View(vehicle);
        }*/
    }
}