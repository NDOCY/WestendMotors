using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WestendMotors.Models;
using WestendMotors.Services;

namespace WestendMotors.Controllers
{
    public class TradeInRequestsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        //private readonly TradeInEmailService _emailService;
        // Local image storage path (relative to application)
        private readonly string imageUploadPath = "~/Content/Uploads/TradeInImages/";

        private readonly TradeInEmailService _emailService;
        private readonly BlobService _blobService;

        public TradeInRequestsController()
        {
            _emailService = new TradeInEmailService();
            _blobService = new BlobService();
        }

        // GET: TradeInRequests
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var userId = Convert.ToInt32(Session["UserId"]);
            var role = Session["Role"]?.ToString();

            if (role == "Customer")
            {
                // Customers only see their own trade-ins
                var customerTradeIns = db.TradeInRequests
                                      .Include(t => t.TargetVehicle)
                                      .Where(t => t.UserId == userId)
                                      .OrderByDescending(t => t.RequestDate)
                                      .ToList();
                return View(customerTradeIns);
            }

            // Admins/Sales see all trade-ins
            var allTradeIns = db.TradeInRequests
                             .Include(t => t.Customer)
                             .Include(t => t.TargetVehicle)
                             .OrderByDescending(t => t.RequestDate)
                             .ToList();

            return View(allTradeIns);
        }

        // Update your UpdateStatus method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateStatus(int id, string status, DateTime? newDate, string notes, decimal? finalOffer)
        {
            var tradeIn = db.TradeInRequests
                .Include(t => t.Customer)
                .FirstOrDefault(t => t.TradeInRequestId == id);

            if (tradeIn == null)
                return HttpNotFound();

            var oldStatus = tradeIn.Status;
            tradeIn.Status = status;
            tradeIn.ScheduledAppointment = newDate;
            tradeIn.AdminNotes = notes;
            tradeIn.FinalOffer = finalOffer;
            tradeIn.AdminReviewDate = DateTime.Now;

            db.SaveChanges();

            // Send status update email
            await _emailService.SendTradeInStatusUpdateAsync(tradeIn, oldStatus, notes);

            TempData["Success"] = "Trade-in request updated and customer notified successfully.";
            return RedirectToAction("AdminReview", new { id = id });
        }

        /*// GET: TradeInRequests/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TradeInRequest tradeInRequest = db.TradeInRequests.Find(id);
            if (tradeInRequest == null)
            {
                return HttpNotFound();
            }
            return View(tradeInRequest);
        }*/

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tradeInRequest = db.TradeInRequests
                .Include(t => t.Customer)
                .Include(t => t.TargetVehicle)
                .Include(t => t.Images)
                .Include(t => t.Appointments)
                .Include(t => t.AssignedStaff) // Include assigned staff
                .FirstOrDefault(t => t.TradeInRequestId == id);

            if (tradeInRequest == null)
            {
                return HttpNotFound();
            }

            // Get available staff for dropdown (Admin users only)
            if (Session["Role"]?.ToString() == "Admin")
            {
                ViewBag.AvailableStaff = new SelectList(
                    db.Users.Where(u => u.IsActive && (u.Role == "Admin" || u.Role == "Sales")),
                    "UserId",
                    "FullName",
                    tradeInRequest.AssignedStaffId
                );
            }

            return View(tradeInRequest);
        }

        // GET: TradeInRequests/Create
        // GET: TradeInRequests/Create?vehicleId=5
        public ActionResult Create(int vehicleId)
        {
            var vehicle = db.Vehicles
                .Include(v => v.Specs)
                .Include(v => v.Images)
                .FirstOrDefault(v => v.Id == vehicleId);

            if (vehicle == null) return HttpNotFound();

            ViewBag.TargetVehicle = vehicle;

            var model = new TradeInRequest
            {
                TargetVehicleId = vehicleId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TradeInRequest tradeInRequest, IEnumerable<HttpPostedFileBase> customerImages)
        {
            if (Session["UserId"] != null)
            {
                tradeInRequest.UserId = Convert.ToInt32(Session["UserId"]);
            }
            else
            {
                ModelState.AddModelError("", "You must be logged in to request a trade-in.");
                return View(tradeInRequest);
            }

            if (ModelState.IsValid)
            {
                // Set request date
                tradeInRequest.RequestDate = DateTime.Now;

                db.TradeInRequests.Add(tradeInRequest);
                db.SaveChanges();

                // Handle file uploads - CHANGED TO AZURE BLOB STORAGE
                if (customerImages != null)
                {
                    foreach (var file in customerImages)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            try
                            {
                                // Validate file type
                                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                                if (!allowedExtensions.Contains(fileExtension))
                                {
                                    ModelState.AddModelError("", $"File {file.FileName} is not a valid image format. Allowed formats: JPG, PNG, GIF, WEBP");
                                    continue;
                                }

                                // Validate file size (e.g., max 10MB)
                                if (file.ContentLength > 10 * 1024 * 1024)
                                {
                                    ModelState.AddModelError("", $"File {file.FileName} is too large. Maximum size is 10MB.");
                                    continue;
                                }

                                // Upload to Azure Blob Storage
                                string imageUrl = await _blobService.UploadTradeInImageAsync(file);

                                db.TradeInImages.Add(new TradeInImage
                                {
                                    TradeInRequestId = tradeInRequest.TradeInRequestId,
                                    ImagePath = imageUrl // Store blob storage URL
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error uploading trade-in image {file.FileName}: {ex.Message}");
                                // Continue with other images even if one fails
                            }
                        }
                    }
                    db.SaveChanges();
                }

                return RedirectToAction("Confirmation");
            }

            ViewBag.TargetVehicle = db.Vehicles
                .Include(v => v.Specs)
                .Include(v => v.Images)
                .FirstOrDefault(v => v.Id == tradeInRequest.TargetVehicleId);

            return View(tradeInRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            TradeInRequest tradeInRequest = db.TradeInRequests
                .Include(t => t.Images)
                .FirstOrDefault(t => t.TradeInRequestId == id);

            if (tradeInRequest != null)
            {
                // Delete associated images from Azure Blob Storage
                foreach (var image in tradeInRequest.Images)
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
                        System.Diagnostics.Debug.WriteLine($"Error deleting trade-in image {image.ImagePath}: {ex.Message}");
                        // Continue with deletion even if image deletion fails
                    }
                }

                db.TradeInRequests.Remove(tradeInRequest);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Confirmation()
        {
            return View();
        }

        // GET: TradeInRequests/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TradeInRequest tradeInRequest = db.TradeInRequests.Find(id);
            if (tradeInRequest == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", tradeInRequest.UserId);
            return View(tradeInRequest);
        }

        // POST: TradeInRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TradeInRequestId,UserId,Make,Model,Year,Mileage,Condition,EstimatedValue,RequestDate")] TradeInRequest tradeInRequest)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tradeInRequest).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", tradeInRequest.UserId);
            return View(tradeInRequest);
        }

        // GET: TradeInRequests/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TradeInRequest tradeInRequest = db.TradeInRequests.Find(id);
            if (tradeInRequest == null)
            {
                return HttpNotFound();
            }
            return View(tradeInRequest);
        }
        /*
        // POST: TradeInRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TradeInRequest tradeInRequest = db.TradeInRequests
                .Include(t => t.Images)
                .FirstOrDefault(t => t.TradeInRequestId == id);

            if (tradeInRequest != null)
            {
                // Delete associated images from local storage
                string physicalPath = Server.MapPath(imageUploadPath);
                foreach (var image in tradeInRequest.Images)
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
                        System.Diagnostics.Debug.WriteLine($"Error deleting trade-in image {image.ImagePath}: {ex.Message}");
                        // Continue with deletion even if image deletion fails
                    }
                }

                db.TradeInRequests.Remove(tradeInRequest);
                db.SaveChanges();
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

        // Update your ScheduleAppointment method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ScheduleAppointment(int tradeInId, DateTime appointmentDate, string notes)
        {
            var tradeIn = db.TradeInRequests
                .Include(t => t.Customer)
                .FirstOrDefault(t => t.TradeInRequestId == tradeInId);

            if (tradeIn == null)
                return HttpNotFound();

            var appointment = new TradeInAppointment
            {
                TradeInRequestId = tradeInId,
                AppointmentDate = appointmentDate,
                Notes = notes,
                Status = "Scheduled",
                CreatedDate = DateTime.Now
            };

            db.TradeInAppointments.Add(appointment);
            tradeIn.Status = "Scheduled";
            tradeIn.ScheduledAppointment = appointmentDate;

            db.SaveChanges();

            // Send appointment scheduled email
            await _emailService.SendTradeInAppointmentScheduledAsync(tradeIn, appointmentDate, notes);

            TempData["Success"] = "Appointment scheduled and customer notified successfully.";
            return RedirectToAction("AdminReview", new { id = tradeInId });
        }

        // GET: TradeInRequests/AdminReview/5
        //[Authorize(Roles = "Admin,Sales")]
        /*
        public ActionResult AdminReview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Load the trade-in request with all related data
            var tradeInRequest = db.TradeInRequests
                .Include(t => t.Customer)
                .Include(t => t.TargetVehicle)
                .Include(t => t.TargetVehicle.Specs)
                .Include(t => t.TargetVehicle.Images)
                .Include(t => t.Images)
                .Include(t => t.Appointments)
                .FirstOrDefault(t => t.TradeInRequestId == id);

            if (tradeInRequest == null)
            {
                return HttpNotFound();
            }

            return View(tradeInRequest);
        }*/
        // Update your AdminReview method to include staff
        /*
        public ActionResult AdminReview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tradeInRequest = db.TradeInRequests
                .Include(t => t.Customer)
                .Include(t => t.TargetVehicle)
                .Include(t => t.TargetVehicle.Specs)
                .Include(t => t.TargetVehicle.Images)
                .Include(t => t.Images)
                .Include(t => t.Appointments)
                .Include(t => t.AssignedStaff) // Include assigned staff
                .Include(t => t.Appointments.Select(a => a.AssignedStaff)) // Include staff for appointments
                .FirstOrDefault(t => t.TradeInRequestId == id);

            if (tradeInRequest == null)
            {
                return HttpNotFound();
            }

            // Get available staff for dropdown
            ViewBag.AvailableStaff = new SelectList(
                db.Users.Where(u => u.IsActive && (u.Role == "Admin" || u.Role == "Sales")),
                "UserId",
                "FullName",
                tradeInRequest.AssignedStaffId
            );

            return View(tradeInRequest);
        }*/

        public ActionResult AdminReview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tradeInRequest = db.TradeInRequests
                .Include(t => t.Customer)
                .Include(t => t.TargetVehicle)
                .Include(t => t.TargetVehicle.Specs)
                .Include(t => t.TargetVehicle.Images)
                .Include(t => t.Images)
                .Include(t => t.Appointments)
                .Include(t => t.AssignedStaff) // Include assigned admin
                .Include(t => t.Appointments.Select(a => a.AssignedStaff)) // Include admin for appointments
                .FirstOrDefault(t => t.TradeInRequestId == id);

            if (tradeInRequest == null)
            {
                return HttpNotFound();
            }

            // Get only Admin users for assignment
            ViewBag.AdminUsers = new SelectList(
                db.Users.Where(u => u.IsActive && u.Role == "Admin"),
                "UserId",
                "FullName",
                tradeInRequest.AssignedStaffId
            );

            return View(tradeInRequest);
        }


        // GET: TradeInRequests/ConvertToVehicle/5
        // GET: TradeInRequests/ConvertToVehicle/5
        public ActionResult ConvertToVehicle(int id)
        {
            var tradeInRequest = db.TradeInRequests
                .Include(t => t.Images)
                .Include(t => t.Customer)
                .FirstOrDefault(t => t.TradeInRequestId == id);

            if (tradeInRequest == null)
            {
                return HttpNotFound();
            }

            // Create view model for conversion with proper default values
            var viewModel = new ConvertTradeInViewModel
            {
                TradeInRequestId = tradeInRequest.TradeInRequestId,
                CustomerId = tradeInRequest.UserId,
                CustomerName = tradeInRequest.Customer != null ? tradeInRequest.Customer.FullName : "Unknown Customer",
                UserId = tradeInRequest.UserId,
                Status = tradeInRequest.Status,
                NumberOfSeats = tradeInRequest.NumberOfSeats,
                Images = tradeInRequest.Images,

                // Vehicle details from trade-in
                Make = tradeInRequest.Make,
                Model = tradeInRequest.Model,
                Year = tradeInRequest.Year,
                Mileage = tradeInRequest.Mileage,
                FuelType = tradeInRequest.FuelType,
                Transmission = tradeInRequest.Transmission,
                Color = tradeInRequest.Color,
                BodyType = tradeInRequest.BodyType,
                ConditionNotes = tradeInRequest.ConditionNotes,

                // Set default values for the form
                Title = $"{tradeInRequest.Year} {tradeInRequest.Make} {tradeInRequest.Model}",
                Description = $"Trade-in vehicle from {tradeInRequest.Customer?.FullName ?? "Unknown Customer"}. Condition: {tradeInRequest.ConditionNotes}",
                Price = tradeInRequest.FinalOffer ?? tradeInRequest.EstimatedValue ?? 0,

                // Pricing
                EstimatedValue = tradeInRequest.EstimatedValue,
                FinalOffer = tradeInRequest.FinalOffer,

                // Assignment options
                AssignToCustomer = true,
                PurchaseDate = DateTime.Today,
                RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" }),
                RecurrenceType = "6 Months",
                NextServiceDate = DateTime.Today.AddMonths(6)
            };

            return View(viewModel);
        }
        /*
        // Add method to assign staff to trade-in request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignStaffToTradeIn(int tradeInId, int staffId)
        {
            var tradeIn = db.TradeInRequests
                .Include(t => t.Customer)
                .Include(t => t.AssignedStaff)
                .FirstOrDefault(t => t.TradeInRequestId == tradeInId);

            var staff = db.Users.Find(staffId);

            if (tradeIn == null || staff == null)
            {
                return HttpNotFound();
            }

            tradeIn.AssignedStaffId = staffId;
            tradeIn.AssignedDate = DateTime.Now;
            db.SaveChanges();

            // Send assignment email
            await _emailService.SendTradeInStaffAssignmentAsync(tradeIn, staff);

            TempData["Success"] = $"Customer assigned to {staff.FullName} successfully. Notification email sent.";
            return RedirectToAction("AdminReview", new { id = tradeInId });
        }

        // Add method to assign staff to trade-in appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignStaffToTradeInAppointment(int appointmentId, int staffId)
        {
            var appointment = db.TradeInAppointments
                .Include(a => a.TradeInRequest)
                .Include(a => a.TradeInRequest.Customer)
                .Include(a => a.AssignedStaff)
                .FirstOrDefault(a => a.Id == appointmentId);

            var staff = db.Users.Find(staffId);

            if (appointment == null || staff == null)
            {
                return HttpNotFound();
            }

            appointment.AssignedStaffId = staffId;
            appointment.AssignedDate = DateTime.Now;
            db.SaveChanges();

            // Send assignment email
            await _emailService.SendTradeInStaffAssignmentAsync(appointment.TradeInRequest, staff);

            TempData["Success"] = $"Appointment assigned to {staff.FullName} successfully. Notification email sent.";
            return RedirectToAction("AdminReview", new { id = appointment.TradeInRequestId });
        }*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignAdminToAppointment(int tradeInId, int appointmentId, int adminId)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var tradeIn = db.TradeInRequests
                        .Include(t => t.Customer)
                        .Include(t => t.AssignedStaff)
                        .Include(t => t.Appointments)
                        .FirstOrDefault(t => t.TradeInRequestId == tradeInId);

                    var appointment = db.TradeInAppointments
                        .Include(a => a.AssignedStaff)
                        .FirstOrDefault(a => a.Id == appointmentId);

                    var adminUser = db.Users.Find(adminId);

                    if (tradeIn == null || appointment == null || adminUser == null)
                    {
                        return HttpNotFound();
                    }

                    // Verify the user is actually an Admin
                    if (adminUser.Role != "Admin")
                    {
                        TempData["Error"] = "Selected user is not an Administrator.";
                        return RedirectToAction("AdminReview", new { id = tradeInId });
                    }

                    // Assign admin to the appointment
                    appointment.AssignedStaffId = adminId;
                    appointment.AssignedDate = DateTime.Now;

                    // Also assign to the main trade-in request (primary assignment)
                    tradeIn.AssignedStaffId = adminId;
                    tradeIn.AssignedDate = DateTime.Now;

                    db.SaveChanges();
                    transaction.Commit();

                    // Send assignment emails
                    await _emailService.SendTradeInAdminAssignmentAsync(tradeIn, adminUser);
                    await _emailService.SendInternalAdminAssignmentNotificationAsync(tradeIn, adminUser);

                    TempData["Success"] = $"{adminUser.FullName} (Admin) has been assigned to the appointment and as primary contact for this trade-in request. Notification emails sent.";
                    return RedirectToAction("AdminReview", new { id = tradeInId });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = $"Error assigning admin: {ex.Message}";
                    return RedirectToAction("AdminReview", new { id = tradeInId });
                }
            }
        }


        /*[HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> ConvertToVehicle(ConvertTradeInViewModel viewModel)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var tradeInRequest = db.TradeInRequests
                        .Include(t => t.Images)
                        .Include(t => t.Customer)
                        .FirstOrDefault(t => t.TradeInRequestId == viewModel.TradeInRequestId);

                    if (tradeInRequest == null)
                    {
                        return HttpNotFound();
                    }

                    // 1. Create the vehicle
                    var vehicle = new Vehicle
                    {
                        Title = $"{viewModel.Year} {viewModel.Make} {viewModel.Model}",
                        Description = $"Trade-in vehicle from {tradeInRequest.Customer?.FullName ?? "Unknown Customer"}. " +
                                     $"Condition: {viewModel.ConditionNotes}",
                        Price = viewModel.Price,
                        IsAvailable = !viewModel.AssignToCustomer, // Available only if not assigned
                        DateAdded = DateTime.Now,
                        Specs = new VehicleSpecs
                        {
                            Make = viewModel.Make,
                            Model = viewModel.Model,
                            Year = viewModel.Year,
                            Mileage = viewModel.Mileage,
                            FuelType = viewModel.FuelType,
                            Transmission = viewModel.Transmission,
                            Color = viewModel.Color,
                            NumberOfSeats = tradeInRequest.NumberOfSeats,
                            BodyType = viewModel.BodyType,
                            ConditionNotes = viewModel.ConditionNotes
                        }
                    };

                    // Copy images from trade-in to vehicle (blob storage approach)
                    foreach (var tradeInImage in tradeInRequest.Images)
                    {
                        try
                        {
                            // For blob storage, we can reference the same image URL
                            // Since both trade-ins and vehicles use blob storage, we can reuse the URL
                            if (tradeInImage.ImagePath.StartsWith("http")) // Already in blob storage
                            {
                                vehicle.Images.Add(new VehicleImage { ImagePath = tradeInImage.ImagePath });
                            }
                            else
                            {
                                // If it's a local file path (legacy data), we need to upload it to blob storage
                                // This would require additional logic to handle local files
                                // For now, we'll skip or log a warning
                                System.Diagnostics.Debug.WriteLine($"Skipping local file: {tradeInImage.ImagePath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing trade-in image: {ex.Message}");
                        }
                    }

                    db.Vehicles.Add(vehicle);
                    db.SaveChanges(); // Save to get vehicle ID

                    // 2. Assign to customer if requested
                    if (viewModel.AssignToCustomer)
                    {
                        var userVehicle = new UserVehicle
                        {
                            UserId = viewModel.CustomerId,
                            VehicleId = vehicle.Id,
                            
                            PurchaseDate = viewModel.PurchaseDate,
                            Notes = viewModel.AssignmentNotes
                        };

                        db.UserVehicles.Add(userVehicle);
                        db.SaveChanges(); // Save to get user vehicle ID

                        // Create service schedule
                        var serviceSchedule = new ServiceSchedule
                        {
                            UserVehicleId = userVehicle.Id,
                            RecurrenceType = viewModel.RecurrenceType,
                            NextServiceDate = viewModel.NextServiceDate,
                            Notes = viewModel.ServiceNotes
                        };

                        db.ServiceSchedules.Add(serviceSchedule);
                    }

                    // 3. Update trade-in status
                    tradeInRequest.Status = "Converted";
                    tradeInRequest.AdminNotes += $"\nConverted to vehicle listing on {DateTime.Now:yyyy-MM-dd}. " +
                                               $"Vehicle ID: {vehicle.Id}" +
                                               (viewModel.AssignToCustomer ? " (Assigned to customer)" : "");

                    db.SaveChanges();
                    transaction.Commit();

                    // Send conversion email
                    await _emailService.SendTradeInConvertedAsync(tradeInRequest, vehicle, viewModel.AssignToCustomer);

                    TempData["SuccessMessage"] = viewModel.AssignToCustomer ?
                        $"Vehicle created and assigned to {tradeInRequest.Customer?.FullName ?? "the customer"} successfully!" :
                        "Vehicle created successfully!";

                    return RedirectToAction("Details", "TradeInRequests", new { id = viewModel.TradeInRequestId });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", $"Error converting trade-in: {ex.Message}");
                    return View(viewModel);
                }
            }
        }*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConvertToVehicle(ConvertTradeInViewModel viewModel)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var tradeInRequest = db.TradeInRequests
                        .Include(t => t.Images)
                        .Include(t => t.Customer)
                        .Include(t => t.TargetVehicle) // Include the target vehicle
                        .FirstOrDefault(t => t.TradeInRequestId == viewModel.TradeInRequestId);

                    if (tradeInRequest == null)
                    {
                        return HttpNotFound();
                    }

                    // 1. Create the NEW vehicle from customer's trade-in (this becomes dealership inventory)
                    var newInventoryVehicle = new Vehicle
                    {
                        Title = viewModel.Title,
                        Description = viewModel.Description,
                        Price = viewModel.Price,
                        IsAvailable = true, // Customer's trade-in becomes available for dealership to sell
                        Status = "Available",
                        DateAdded = DateTime.Now,
                        Specs = new VehicleSpecs
                        {
                            Make = viewModel.Make,
                            Model = viewModel.Model,
                            Year = viewModel.Year,
                            Mileage = viewModel.Mileage,
                            FuelType = viewModel.FuelType,
                            Transmission = viewModel.Transmission,
                            Color = viewModel.Color,
                            EngineSize = viewModel.EngineSize,
                            NumberOfSeats = viewModel.NumberOfSeats,
                            BodyType = viewModel.BodyType,
                            ConditionNotes = viewModel.ConditionNotes,
                            FeatureList = viewModel.FeatureList
                        }
                    };

                    // Copy images from trade-in to the new dealership vehicle
                    foreach (var tradeInImage in tradeInRequest.Images)
                    {
                        try
                        {
                            if (tradeInImage.ImagePath.StartsWith("http"))
                            {
                                newInventoryVehicle.Images.Add(new VehicleImage { ImagePath = tradeInImage.ImagePath });
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing trade-in image: {ex.Message}");
                        }
                    }

                    db.Vehicles.Add(newInventoryVehicle);
                    db.SaveChanges(); // Save to get the new vehicle ID

                    // 2. Handle customer assignment to the TARGET vehicle (the one they want)
                    if (viewModel.AssignToCustomer && tradeInRequest.TargetVehicle != null)
                    {
                        // Mark the target vehicle as unavailable (sold to customer)
                        tradeInRequest.TargetVehicle.IsAvailable = false;
                        tradeInRequest.TargetVehicle.Status = "Sold";

                        // Create user vehicle assignment
                        var userVehicle = new UserVehicle
                        {
                            UserId = viewModel.CustomerId,
                            VehicleId = tradeInRequest.TargetVehicle.Id, // Assign the TARGET vehicle to customer
                            PurchaseDate = viewModel.PurchaseDate,
                            Notes = viewModel.AssignmentNotes
                        };

                        db.UserVehicles.Add(userVehicle);
                        db.SaveChanges(); // Save to get user vehicle ID

                        // Create service schedule for the target vehicle
                        var serviceSchedule = new ServiceSchedule
                        {
                            UserVehicleId = userVehicle.Id,
                            RecurrenceType = viewModel.RecurrenceType,
                            NextServiceDate = viewModel.NextServiceDate,
                            Notes = viewModel.ServiceNotes
                        };

                        db.ServiceSchedules.Add(serviceSchedule);
                    }

                    // 3. Update trade-in status
                    tradeInRequest.Status = "Converted";
                    tradeInRequest.AdminNotes += $"\nTrade-in converted on {DateTime.Now:yyyy-MM-dd}. " +
                                               $"Customer's vehicle added to inventory (ID: {newInventoryVehicle.Id})" +
                                               (viewModel.AssignToCustomer ?
                                                   $". Target vehicle (ID: {tradeInRequest.TargetVehicleId}) assigned to customer." :
                                                   ". No vehicle assigned to customer.");

                    db.SaveChanges();
                    transaction.Commit();

                    // Send conversion email
                    await _emailService.SendTradeInConvertedAsync(tradeInRequest, newInventoryVehicle, viewModel.AssignToCustomer);

                    TempData["SuccessMessage"] = viewModel.AssignToCustomer ?
                        $"Trade-in completed! Customer's vehicle added to inventory. Target vehicle assigned to {tradeInRequest.Customer?.FullName ?? "the customer"}." :
                        $"Trade-in completed! Customer's vehicle added to inventory (not assigned to customer).";

                    return RedirectToAction("Details", "TradeInRequests", new { id = viewModel.TradeInRequestId });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", $"Error converting trade-in: {ex.Message}");

                    // Repopulate the view model if there's an error
                    var tradeInRequest = db.TradeInRequests
                        .Include(t => t.Images)
                        .Include(t => t.Customer)
                        .FirstOrDefault(t => t.TradeInRequestId == viewModel.TradeInRequestId);

                    if (tradeInRequest != null)
                    {
                        viewModel.CustomerName = tradeInRequest.Customer?.FullName ?? "Unknown Customer";
                    }

                    return View(viewModel);
                }
            }
        }
    }
}