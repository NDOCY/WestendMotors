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
    public class AppointmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

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
        public ActionResult Create(int? vehicleId, string vehicleTitle)
        {
            // Check if user is logged in
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var appointment = new Appointment();

            // If vehicleId is provided, pre-populate the vehicle info
            if (vehicleId.HasValue)
            {
                appointment.VehicleId = vehicleId.Value;
                appointment.AppointmentType = "Test Drive";
                ViewBag.VehicleTitle = vehicleTitle ?? db.Vehicles.Find(vehicleId.Value)?.Title;
            }
            else
            {
                // For general appointments, set default type or let user choose
                appointment.AppointmentType = "Consultation";
                ViewBag.VehicleTitle = "Select a Vehicle";

                // Populate vehicles for dropdown
                ViewBag.Vehicles = db.Vehicles.ToList();
            }

            return View(appointment);
        }


        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AppointmentId,VehicleId,AppointmentType,AppointmentDate,Notes")] Appointment appointment)
        {
            if (appointment.AppointmentDate < DateTime.Now)
            {
                ModelState.AddModelError("AppointmentDate", "Appointment date must be in the future.");
            }

            if (Session["UserId"] == null)
            {
                ModelState.AddModelError("", "You must be logged in to book an appointment.");
                ViewBag.VehicleTitle = db.Vehicles.Find(appointment.VehicleId)?.Title ?? "Vehicle";
                return View(appointment);
            }

            var userId = Convert.ToInt32(Session["UserId"]);

            // Check for existing pending appointment for this user and vehicle
            var existingAppointment = db.Appointments
                .Any(a => a.UserId == userId &&
                         a.VehicleId == appointment.VehicleId &&
                         a.Status == "Pending");

            if (existingAppointment)
            {
                ModelState.AddModelError("", "You already have a pending appointment for this vehicle. Please wait for it to be processed or contact support.");
                ViewBag.VehicleTitle = db.Vehicles.Find(appointment.VehicleId)?.Title ?? "Vehicle";
                return View(appointment);
            }

            if (ModelState.IsValid)
            {
                appointment.UserId = userId;
                appointment.Status = "Pending";

                db.Appointments.Add(appointment);
                db.SaveChanges();

                return RedirectToAction("Confirmation");
            }

            ViewBag.VehicleTitle = db.Vehicles.Find(appointment.VehicleId)?.Title ?? "Vehicle";
            return View(appointment);
        }


        public ActionResult Confirmation()
        {
            return View();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status, string adminNotes, DateTime? rescheduledDate)
        {
            var appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }

            appointment.Status = status;
            appointment.AdminNotes = adminNotes;

            if (status == "Postponed" && rescheduledDate.HasValue)
            {
                appointment.RescheduledDate = rescheduledDate;
            }

            db.SaveChanges();

            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reschedule(int id, DateTime newDate, string rescheduleNotes)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var appointment = db.Appointments.Find(id);
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

            TempData["Success"] = "Reschedule request submitted successfully.";
            return RedirectToAction("Details", new { id = rescheduledAppointment.AppointmentId });
        }

    }
}
