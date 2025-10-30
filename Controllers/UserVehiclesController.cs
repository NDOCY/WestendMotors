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
    public class UserVehiclesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: UserVehicles
        // GET: UserVehicles
        public ActionResult Index()
        {
            // Change from .Include(u => u.ServiceSchedule) to .Include(u => u.ServiceSchedules)
            var userVehicles = db.UserVehicles
                .Include(u => u.ServiceSchedules)  // Changed to plural
                .Include(u => u.User)
                .Include(u => u.Vehicle);

            return View(userVehicles.ToList());
        }

        // GET: UserVehicles/Details/5
        // GET: UserVehicles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Include the ServiceSchedules collection
            UserVehicle userVehicle = db.UserVehicles
                .Include(u => u.ServiceSchedules)  // Added this include
                .Include(u => u.User)
                .Include(u => u.Vehicle)
                .FirstOrDefault(u => u.Id == id);

            if (userVehicle == null)
            {
                return HttpNotFound();
            }

            return View(userVehicle);
        
        }

        // GET: UserVehicles/Create
        public ActionResult Create()
        {
            // Remove the ServiceSchedule dropdown since it's now a collection
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName");
            ViewBag.VehicleId = new SelectList(db.Vehicles, "Id", "Title");
            return View();
        }

        // POST: UserVehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,VehicleId,PurchaseDate,Notes")] UserVehicle userVehicle)
        {
            if (ModelState.IsValid)
            {
                db.UserVehicles.Add(userVehicle);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Remove ServiceSchedule from ViewBag
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", userVehicle.UserId);
            ViewBag.VehicleId = new SelectList(db.Vehicles, "Id", "Title", userVehicle.VehicleId);
            return View(userVehicle);
        }

        // GET: UserVehicles/Edit/5
        // GET: UserVehicles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            UserVehicle userVehicle = db.UserVehicles.Find(id);
            if (userVehicle == null)
            {
                return HttpNotFound();
            }

            // Remove ServiceSchedule from ViewBag
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", userVehicle.UserId);
            ViewBag.VehicleId = new SelectList(db.Vehicles, "Id", "Title", userVehicle.VehicleId);
            return View(userVehicle);
        }

        // POST: UserVehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,VehicleId,PurchaseDate,Notes")] UserVehicle userVehicle)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userVehicle).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Remove ServiceSchedule from ViewBag
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", userVehicle.UserId);
            ViewBag.VehicleId = new SelectList(db.Vehicles, "Id", "Title", userVehicle.VehicleId);
            return View(userVehicle);
        }

        // GET: UserVehicles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserVehicle userVehicle = db.UserVehicles.Find(id);
            if (userVehicle == null)
            {
                return HttpNotFound();
            }
            return View(userVehicle);
        }

        // POST: UserVehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserVehicle userVehicle = db.UserVehicles.Find(id);
            db.UserVehicles.Remove(userVehicle);
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

        // GET: Assign vehicle to user
        public ActionResult Assign()
        {
            ViewBag.UserId = new SelectList(db.Users, "Id", "FullName");
            ViewBag.VehicleId = new SelectList(db.Vehicles.Where(v => v.IsAvailable), "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Assign(int userId, int vehicleId)
        {
            var userVehicle = new UserVehicle
            {
                UserId = userId,
                VehicleId = vehicleId,
                PurchaseDate = DateTime.Now
            };

            // Mark vehicle as unavailable since it's sold/assigned
            var vehicle = db.Vehicles.Find(vehicleId);
            if (vehicle != null)
            {
                vehicle.IsAvailable = false;
                db.Entry(vehicle).State = EntityState.Modified;
            }

            db.UserVehicles.Add(userVehicle);
            db.SaveChanges();

            return RedirectToAction("SetServiceSchedule", new { userVehicleId = userVehicle.Id });
        }

        // GET: Set service schedule for assigned vehicle
        /*public ActionResult SetServiceSchedule(int userVehicleId)
        {
            var userVehicle = db.UserVehicles.Include(uv => uv.Vehicle).Include(uv => uv.User).FirstOrDefault(uv => uv.Id == userVehicleId);
            if (userVehicle == null) return HttpNotFound();

            ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" });
            var model = new ServiceSchedule { UserVehicleId = userVehicleId, NextServiceDate = DateTime.Now.AddMonths(1) };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetServiceSchedule(ServiceSchedule schedule)
        {
            if (ModelState.IsValid)
            {
                // Calculate next service date based on recurrence type
                schedule.NextServiceDate = CalculateNextServiceDate(DateTime.Now, schedule.RecurrenceType);

                db.ServiceSchedules.Add(schedule);
                db.SaveChanges();

                return RedirectToAction("Index", "ServiceRecords");
            }
            ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" }, schedule.RecurrenceType);
            return View(schedule);
        }*/

        // GET: Set service schedule for assigned vehicle
        public ActionResult SetServiceSchedule(int userVehicleId)
        {
            var userVehicle = db.UserVehicles
                .Include(uv => uv.Vehicle)
                .Include(uv => uv.User)
                .FirstOrDefault(uv => uv.Id == userVehicleId);

            if (userVehicle == null) return HttpNotFound();

            ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" });

            // Create a new service schedule (note: this will add to the collection)
            var model = new ServiceSchedule
            {
                UserVehicleId = userVehicleId,
                NextServiceDate = DateTime.Now.AddMonths(1)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetServiceSchedule(ServiceSchedule schedule)
        {
            if (ModelState.IsValid)
            {
                // Calculate next service date based on recurrence type
                schedule.NextServiceDate = CalculateNextServiceDate(DateTime.Now, schedule.RecurrenceType);

                db.ServiceSchedules.Add(schedule);
                db.SaveChanges();

                return RedirectToAction("Index", "ServiceRecords");
            }

            ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" }, schedule.RecurrenceType);
            return View(schedule);
        }

        private DateTime CalculateNextServiceDate(DateTime startDate, string recurrenceType)
        {
            switch (recurrenceType)
            {
                case "Monthly": return startDate.AddMonths(1);
                case "Quarterly": return startDate.AddMonths(3);
                case "6 Months": return startDate.AddMonths(6);
                case "Yearly": return startDate.AddYears(1);
                default: throw new ArgumentException("Invalid recurrence type");
            }
        }


    }
}
