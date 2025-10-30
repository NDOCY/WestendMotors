using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WestendMotors.Models;
using WestendMotors.Services;

namespace WestendMotors.Controllers
{
    public class AppointmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private readonly AppointmentEmailService _emailService;

        public AppointmentsController()
        {
            _emailService = new AppointmentEmailService();
        }

        // GET: Appointments
        /*public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var userId = Convert.ToInt32(Session["UserId"]);
            var role = Session["Role"]?.ToString();

            if (role == "Customer")
            {
                // Customers only see their own appointments
                var customerAppointments = db.Appointments
                                          .Include(a => a.Vehicle)
                                          .Where(a => a.UserId == userId)
                                          .ToList();
                return View(customerAppointments);
            }

            // Admins/Sales see all appointments
            var allAppointments = db.Appointments
                                 .Include(a => a.Customer)
                                 .Include(a => a.Vehicle)
                                 .ToList();
            return View(allAppointments);
        }*/

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
                var customerAppointments = db.Appointments
                                          .Include(a => a.Vehicle)
                                          .Include(a => a.OriginalAppointment)
                                          .Where(a => a.UserId == userId)
                                          .OrderByDescending(a => a.AppointmentDate)
                                          .ToList();
                return View(customerAppointments);
            }

            var allAppointments = db.Appointments
                                 .Include(a => a.Customer)
                                 .Include(a => a.Vehicle)
                                 .Include(a => a.OriginalAppointment)
                                 .OrderByDescending(a => a.AppointmentDate)
                                 .ToList();
            return View(allAppointments);
        }

        // GET: Appointments/Details/5
        /*
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }*/
        // Update your Details method to include staff
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var appointment = db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .Include(a => a.AssignedStaff) // Include assigned staff
                .Include(a => a.OriginalAppointment)
                .Include(a => a.Vehicle.Images) // Include vehicle images
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            // Get available staff for dropdown (Admin and Sales roles)
            ViewBag.AvailableStaff = new SelectList(
                db.Users.Where(u => u.IsActive && (u.Role == "Admin" || u.Role == "Sales")),
                "UserId",
                "FullName",
                appointment.AssignedStaffId
            );

            return View(appointment);
        }

        // GET: Appointments/Create
        // GET: Appointments/Create?vehicleId=5&vehicleTitle=Toyota%20Camry
        // [Authorize]
        /*public ActionResult Create(int vehicleId, string vehicleTitle)
        {
            var appointment = new Appointment
            {
                VehicleId = vehicleId,
                AppointmentType = "Test Drive"
            };

            ViewBag.VehicleTitle = vehicleTitle;

            return View(appointment);
        }*/

        // GET: Appointments/Create
        // [Authorize]
        // GET: Appointments/Create
        // GET: Appointments/Create
        public ActionResult Create(int? vehicleId, string appointmentType, string serviceType)
        {
            // Check if user is logged in
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var appointment = new Appointment();

            // Determine the appointment type and behavior
            if (vehicleId.HasValue)
            {
                appointment.VehicleId = vehicleId.Value;
                var vehicle = db.Vehicles.Find(vehicleId.Value);
                ViewData["VehicleTitle"] = vehicle?.Title;

                // Set appointment type based on parameter or default to Test Drive
                appointment.AppointmentType = !string.IsNullOrEmpty(appointmentType) ? appointmentType : "Test Drive";

                // If it's a service appointment, set service type if provided
                if (appointment.AppointmentType == "Service" && !string.IsNullOrEmpty(serviceType))
                {
                    appointment.ServiceType = serviceType;
                }
            }
            else
            {
                // For general appointments without vehicle
                appointment.AppointmentType = !string.IsNullOrEmpty(appointmentType) ? appointmentType : "Consultation";
                ViewData["VehicleTitle"] = "Select a Vehicle (Optional)";

                // Populate vehicles for dropdown (optional selection)
                ViewData["Vehicles"] = new SelectList(db.Vehicles, "Id", "Title");
            }

            // Pass service types for dropdown if it's a service appointment
            if (appointment.AppointmentType == "Service")
            {
                ViewData["ServiceTypes"] = new SelectList(GetServiceTypes(), appointment.ServiceType);
            }

            ViewData["AppointmentType"] = appointment.AppointmentType;
            return View(appointment);
        }

        // POST: Appointments/Create
        // Update your Create method to send confirmation email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Appointment appointment) // Changed to async Task
        {
            if (appointment.AppointmentDate < DateTime.Now)
            {
                ModelState.AddModelError("AppointmentDate", "Appointment date must be in the future.");
            }

            if (Session["UserId"] == null)
            {
                ModelState.AddModelError("", "You must be logged in to book an appointment.");
                return View(appointment);
            }

            var userId = Convert.ToInt32(Session["UserId"]);

            // Check for existing pending appointment
            if (appointment.VehicleId.HasValue)
            {
                var existingAppointment = db.Appointments
                    .Any(a => a.UserId == userId &&
                             a.VehicleId == appointment.VehicleId &&
                             a.AppointmentType == appointment.AppointmentType &&
                             a.Status == "Pending");

                if (existingAppointment)
                {
                    ModelState.AddModelError("", $"You already have a pending {appointment.AppointmentType.ToLower()} appointment for this vehicle.");
                    return View(appointment);
                }
            }

            if (ModelState.IsValid)
            {
                appointment.UserId = userId;
                appointment.Status = "Pending";

                db.Appointments.Add(appointment);
                db.SaveChanges();

                // Load customer and vehicle details for email
                var appointmentWithDetails = db.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Vehicle)
                    .FirstOrDefault(a => a.AppointmentId == appointment.AppointmentId);

                // Send confirmation email
                if (appointmentWithDetails != null)
                {
                    await _emailService.SendAppointmentConfirmationAsync(appointmentWithDetails);
                }

                return RedirectToAction("Confirmation", new { id = appointment.AppointmentId });
            }

            // Repopulate view data if validation fails
            if (appointment.VehicleId.HasValue)
            {
                var vehicle = db.Vehicles.Find(appointment.VehicleId.Value);
                ViewBag.VehicleTitle = vehicle?.Title;
            }
            else
            {
                ViewBag.Vehicles = new SelectList(db.Vehicles, "Id", "Title", appointment.VehicleId);
            }

            if (appointment.AppointmentType == "Service")
            {
                ViewBag.ServiceTypes = new SelectList(GetServiceTypes(), appointment.ServiceType);
            }

            ViewBag.AppointmentType = appointment.AppointmentType;
            return View(appointment);
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


        public ActionResult Confirmation(int id)
        {
            var appointment = db.Appointments
                .Include(a => a.Vehicle)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            ViewBag.AppointmentType = appointment.AppointmentType;
            return View(appointment);
        }



        // GET: Appointments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", appointment.UserId);
            ViewBag.VehicleId = new SelectList(db.Vehicles, "VehicleId", "Make", appointment.VehicleId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AppointmentId,UserId,VehicleId,AppointmentType,AppointmentDate,Status,Notes")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", appointment.UserId);
            ViewBag.VehicleId = new SelectList(db.Vehicles, "VehicleId", "Make", appointment.VehicleId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Appointment appointment = db.Appointments.Find(id);
            db.Appointments.Remove(appointment);
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

        /*[HttpPost]
        [ValidateAntiForgeryToken]
        // [Authorize(Roles = "Admin")] // Uncomment if you have role-based auth
        public ActionResult UpdateStatus(int id, string status)
        {
            var appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }

            appointment.Status = status;
            db.SaveChanges();

            return RedirectToAction("Details", new { id = id });
        }*/

        // Update your UpdateStatus method
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateStatus(int id, string status, string adminNotes, DateTime? rescheduledDate)
        {
            var appointment = db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            var oldStatus = appointment.Status;
            appointment.Status = status;
            appointment.AdminNotes = adminNotes;

            if (status == "Postponed" && rescheduledDate.HasValue)
            {
                appointment.RescheduledDate = rescheduledDate;
            }

            db.SaveChanges();

            // Send status update email
            await _emailService.SendAppointmentStatusUpdateAsync(appointment, oldStatus, adminNotes);

            return RedirectToAction("Details", new { id = id });
        }*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateStatus(int id, string status, string adminNotes, DateTime? rescheduledDate, int? assignStaffId = null)
        {
            var appointment = db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .Include(a => a.AssignedStaff)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            var oldStatus = appointment.Status;
            appointment.Status = status;
            appointment.AdminNotes = adminNotes;

            // Assign staff if provided
            if (assignStaffId.HasValue)
            {
                var staff = db.Users.Find(assignStaffId.Value);
                if (staff != null)
                {
                    appointment.AssignedStaffId = assignStaffId.Value;
                    appointment.AssignedDate = DateTime.Now;
                }
            }

            if (status == "Postponed" && rescheduledDate.HasValue)
            {
                appointment.RescheduledDate = rescheduledDate;
            }

            db.SaveChanges();

            // Send status update email
            await _emailService.SendAppointmentStatusUpdateAsync(appointment, oldStatus, adminNotes);

            // Send staff assignment email if staff was assigned
            if (assignStaffId.HasValue && appointment.AssignedStaff != null)
            {
                await _emailService.SendAppointmentStaffAssignmentAsync(appointment, appointment.AssignedStaff);
                await _emailService.SendInternalStaffAssignmentNotificationAsync(appointment, appointment.AssignedStaff);
            }

            return RedirectToAction("Details", new { id = id });
        }

        // Update your Reschedule method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reschedule(int id, DateTime newDate, string rescheduleNotes)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var appointment = db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            var userId = Convert.ToInt32(Session["UserId"]);
            if (appointment.UserId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (newDate < DateTime.Now)
            {
                TempData["Error"] = "Rescheduled date must be in the future.";
                return RedirectToAction("Details", new { id = id });
            }

            // Create a new appointment for rescheduling
            var rescheduledAppointment = new Appointment
            {
                UserId = appointment.UserId,
                VehicleId = appointment.VehicleId,
                AppointmentType = appointment.AppointmentType,
                AppointmentDate = newDate,
                Notes = rescheduleNotes,
                Status = "Pending",
                OriginalAppointmentId = appointment.AppointmentId
            };

            // Update original appointment status
            appointment.Status = "Reschedule Requested";
            appointment.AdminNotes = $"Customer requested reschedule. New date requested: {newDate}. Reason: {rescheduleNotes}";

            db.Appointments.Add(rescheduledAppointment);
            db.SaveChanges();

            // Send reschedule request email
            await _emailService.SendRescheduleRequestAsync(appointment, rescheduledAppointment, rescheduleNotes);

            TempData["Success"] = "Reschedule request submitted successfully. You will receive a confirmation email shortly.";
            return RedirectToAction("Details", new { id = rescheduledAppointment.AppointmentId });
        }

        // Add method to assign staff to appointment
        /* [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<ActionResult> AssignStaffToAppointment(int appointmentId, int staffId)
         {
             var appointment = db.Appointments
                 .Include(a => a.Customer)
                 .Include(a => a.AssignedStaff)
                 .Include(a => a.Vehicle)
                 .FirstOrDefault(a => a.AppointmentId == appointmentId);

             var staff = db.Users.Find(staffId);

             if (appointment == null || staff == null)
             {
                 return HttpNotFound();
             }

             appointment.AssignedStaffId = staffId;
             appointment.AssignedDate = DateTime.Now;
             db.SaveChanges();

             // Send assignment email
             //await _emailService.SendAppointmentStaffAssignmentAsync(appointment, staff);

             if (Request.IsAjaxRequest())
             {
                 return Json(new { success = true, message = $"Appointment assigned to {staff.FullName} successfully." });
             }

             TempData["Success"] = $"Appointment assigned to {staff.FullName} successfully. Notification email sent.";
             return RedirectToAction("Details", new { id = appointmentId });

             //TempData["Success"] = $"Appointment assigned to {staff.FullName} successfully. Notification email sent.";
             //return RedirectToAction("Details", new { id = appointmentId });
         }*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignStaffToAppointment(int appointmentId, int staffId)
        {
            var appointment = db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.AssignedStaff)
                .Include(a => a.Vehicle)
                .FirstOrDefault(a => a.AppointmentId == appointmentId);

            var staff = db.Users.Find(staffId);

            if (appointment == null || staff == null)
            {
                return HttpNotFound();
            }

            appointment.AssignedStaffId = staffId;
            appointment.AssignedDate = DateTime.Now;
            db.SaveChanges();

            // Send assignment emails (UNCOMMENT AND FIX THIS)
            await _emailService.SendAppointmentStaffAssignmentAsync(appointment, staff);
            await _emailService.SendInternalStaffAssignmentNotificationAsync(appointment, staff);

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, message = $"Appointment assigned to {staff.FullName} successfully." });
            }

            TempData["Success"] = $"Appointment assigned to {staff.FullName} successfully. Notification email sent.";
            return RedirectToAction("Details", new { id = appointmentId });
        }

    }
}
