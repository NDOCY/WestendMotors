using System;
using System.Collections.Generic;
using System.Configuration;
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
    public class TradeInRequestsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin,Sales")]
        public ActionResult UpdateStatus(int id, string status, DateTime? newDate, string notes, decimal? finalOffer)
        {
            var tradeIn = db.TradeInRequests.Find(id);
            if (tradeIn == null)
                return HttpNotFound();

            tradeIn.Status = status;
            tradeIn.ScheduledAppointment = newDate;
            tradeIn.AdminNotes = notes;
            tradeIn.FinalOffer = finalOffer;
            tradeIn.AdminReviewDate = DateTime.Now;

            db.SaveChanges();

            TempData["Success"] = "Trade-in request updated successfully.";
            return RedirectToAction("AdminReview", new { id = id });
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
                db.TradeInRequests.Add(tradeInRequest);
                db.SaveChanges();

                // Handle file uploads - ONLY THIS PART CHANGES
                if (customerImages != null)
                {
                    var blobService = new BlobService(ConfigurationManager.AppSettings["AzureBlobConnection"]);

                    foreach (var file in customerImages)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            // Upload to Azure Blob Storage instead of local
                            string imageUrl = await blobService.UploadImageAsync(file);

                            db.TradeInImages.Add(new TradeInImage
                            {
                                TradeInRequestId = tradeInRequest.TradeInRequestId,
                                ImagePath = imageUrl // This now stores the Azure URL
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin,Sales")]
        public ActionResult ScheduleAppointment(int tradeInId, DateTime appointmentDate, string notes)
        {
            var appointment = new TradeInAppointment
            {
                TradeInRequestId = tradeInId,
                AppointmentDate = appointmentDate,
                Notes = notes,
                Status = "Scheduled",
                CreatedDate = DateTime.Now
            };

            db.TradeInAppointments.Add(appointment);

            // Update the trade-in status
            var tradeIn = db.TradeInRequests.Find(tradeInId);
            tradeIn.Status = "Scheduled";
            tradeIn.ScheduledAppointment = appointmentDate;

            db.SaveChanges();

            TempData["Success"] = "Appointment scheduled successfully.";
            return RedirectToAction("AdminReview", new { id = tradeInId });
        }

        // GET: TradeInRequests/AdminReview/5
        //[Authorize(Roles = "Admin,Sales")]
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
        }
    }
}
