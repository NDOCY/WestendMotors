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
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Users
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,FullName,Email,PasswordHash,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,FullName,Email,PasswordHash,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
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

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            // You'd normally hash the password before comparison
            var user = db.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);

            if (user != null)
            {
                // Store in session
                Session["UserId"] = user.UserId;
                Session["FullName"] = user.FullName;
                Session["Role"] = user.Role;

                // Redirect based on role
                if (user.Role == "Admin")
                    return RedirectToAction("Admin", "Dashboard");
                else
                    return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }
        // GET: Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string fullName, string email, string password, string role = "Customer")
        {
            // Check if email is already in use
            if (db.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "Email is already registered.";
                return View();
            }

            // In production, hash the password before saving
            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = password,
                Role = role
            };

            db.Users.Add(user);
            db.SaveChanges();

            // Automatically log them in after registration
            Session["UserId"] = user.UserId;
            Session["FullName"] = user.FullName;
            Session["Role"] = user.Role;

            // Redirect based on role
            if (user.Role == "Admin")
                return RedirectToAction("Admin", "Dashboard");
            else
                return RedirectToAction("Index", "Dashboard");
        }


        // Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        public ActionResult AssignVehicle(int userId)
        {
            ViewBag.Vehicles = new SelectList(db.Vehicles.Where(v => v.IsAvailable), "Id", "Title");
            ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" });

            var model = new AssignVehicleViewModel
            {
                UserId = userId,
                PurchaseDate = DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignVehicle(AssignVehicleViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get the selected vehicle
                var vehicle = db.Vehicles.FirstOrDefault(v => v.Id == model.VehicleId);
                if (vehicle == null)
                {
                    ModelState.AddModelError("", "Selected vehicle not found.");
                }
                else
                {
                    // Mark vehicle as unavailable
                    vehicle.IsAvailable = false;

                    // Create UserVehicle
                    var userVehicle = new UserVehicle
                    {
                        UserId = model.UserId,
                        VehicleId = model.VehicleId,
                        PurchaseDate = model.PurchaseDate,
                        Notes = model.Notes
                    };

                    // Create ServiceSchedule
                    var serviceSchedule = new ServiceSchedule
                    {
                        RecurrenceType = model.RecurrenceType,
                        NextServiceDate = model.NextServiceDate,
                        Notes = model.ServiceNotes,
                        UserVehicle = userVehicle
                    };

                    db.UserVehicles.Add(userVehicle);
                    db.ServiceSchedules.Add(serviceSchedule);

                    // Save all changes including IsAvailable update
                    db.SaveChanges();

                    return RedirectToAction("Details", "Users", new { id = model.UserId });
                }
            }

            ViewBag.Vehicles = new SelectList(db.Vehicles.Where(v => v.IsAvailable), "Id", "Title", model.VehicleId);
            ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" }, model.RecurrenceType);
            return View(model);
        }



    }
}
