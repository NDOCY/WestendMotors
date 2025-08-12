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

        // GET: ServiceRecords
        public ActionResult Index()
        {
            var serviceRecords = db.ServiceRecords.Include(s => s.Vehicle);
            return View(serviceRecords.ToList());
        }

        // GET: ServiceRecords/Details/5
        public ActionResult Details(int? id)
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
            return View(serviceRecord);
        }

        // GET: ServiceRecords/Create
        public ActionResult Create()
        {
            ViewBag.VehicleId = new SelectList(db.Vehicles, "VehicleId", "Make");
            return View();
        }

        // POST: ServiceRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ServiceRecordId,VehicleId,ServiceDate,Description,Cost")] ServiceRecord serviceRecord)
        {
            if (ModelState.IsValid)
            {
                db.ServiceRecords.Add(serviceRecord);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.VehicleId = new SelectList(db.Vehicles, "VehicleId", "Make", serviceRecord.VehicleId);
            return View(serviceRecord);
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
            ViewBag.VehicleId = new SelectList(db.Vehicles, "VehicleId", "Make", serviceRecord.VehicleId);
            return View(serviceRecord);
        }

        // POST: ServiceRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ServiceRecordId,VehicleId,ServiceDate,Description,Cost")] ServiceRecord serviceRecord)
        {
            if (ModelState.IsValid)
            {
                db.Entry(serviceRecord).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.VehicleId = new SelectList(db.Vehicles, "VehicleId", "Make", serviceRecord.VehicleId);
            return View(serviceRecord);
        }

        // GET: ServiceRecords/Delete/5
        public ActionResult Delete(int? id)
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
            return View(serviceRecord);
        }

        // POST: ServiceRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ServiceRecord serviceRecord = db.ServiceRecords.Find(id);
            db.ServiceRecords.Remove(serviceRecord);
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

        public DateTime CalculateNextServiceDate(DateTime startDate, string recurrenceType)
        {
            switch (recurrenceType)
            {
                case "Monthly":
                    return startDate.AddMonths(1);
                case "Quarterly":
                    return startDate.AddMonths(3);
                case "6 Months":
                    return startDate.AddMonths(6);
                case "Yearly":
                    return startDate.AddYears(1);
                default:
                    throw new ArgumentException("Invalid recurrence type");
            }
        }

    }
}
