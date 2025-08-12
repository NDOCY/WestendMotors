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
        public ActionResult Index()
        {
            var appointments = db.Appointments.Include(a => a.Customer).Include(a => a.Vehicle);
            return View(appointments.ToList());
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
        public ActionResult Create(int vehicleId, string vehicleTitle)
        {
            var appointment = new Appointment
            {
                VehicleId = vehicleId,
                AppointmentType = "Test Drive"
            };

            ViewBag.VehicleTitle = vehicleTitle;

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

            if (ModelState.IsValid)
            {
                // ✅ Get logged-in user ID from session
                if (Session["UserId"] != null)
                {
                    appointment.UserId = Convert.ToInt32(Session["UserId"]);
                }
                else
                {
                    ModelState.AddModelError("", "You must be logged in to book an appointment.");
                    ViewBag.VehicleTitle = db.Vehicles.Find(appointment.VehicleId)?.Title ?? "Vehicle";
                    return View(appointment);
                }

                // Default status
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

        [HttpPost]
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
        }

    }
}
