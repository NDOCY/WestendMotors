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
    public class TradeInRequestsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: TradeInRequests

        public ActionResult Index()
        {
            var tradeIns = db.TradeInRequests
                .Include(t => t.Customer)
                .Include(t => t.TargetVehicle)
                .OrderByDescending(t => t.RequestDate)
                .ToList();

            return View(tradeIns);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status, DateTime? newDate, string notes)
        {
            var tradeIn = db.TradeInRequests.Find(id);
            if (tradeIn == null)
                return HttpNotFound();

            tradeIn.Status = status;
            tradeIn.NewAppointmentDate = newDate;
            tradeIn.AdminNotes = notes;

            db.SaveChanges();

            TempData["Success"] = "Trade-in request updated successfully.";
            return RedirectToAction("Details", new { id = id });
        }



        // GET: TradeInRequests/Details/5
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
        public ActionResult Create(TradeInRequest tradeInRequest, IEnumerable<HttpPostedFileBase> customerImages)
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
                db.TradeInRequests.Add(tradeInRequest);
                db.SaveChanges();

                // Handle file uploads
                if (customerImages != null)
                {
                    foreach (var file in customerImages)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            var fileName = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                            var path = Server.MapPath("~/Uploads/TradeIns/" + fileName);
                            file.SaveAs(path);

                            db.Set<TradeInImage>().Add(new TradeInImage
                            {
                                TradeInRequestId = tradeInRequest.TradeInRequestId,
                                ImagePath = "/Uploads/TradeIns/" + fileName
                            });
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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

        // POST: TradeInRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TradeInRequest tradeInRequest = db.TradeInRequests.Find(id);
            db.TradeInRequests.Remove(tradeInRequest);
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
