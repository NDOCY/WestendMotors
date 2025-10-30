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
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly VehicleAssignmentEmailService _emailService;

        public UsersController()
        {
            _emailService = new VehicleAssignmentEmailService();
        }

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
                Role = "Customer"
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
            var user = db.Users.Find(userId);
            if (user == null)
            {
                return HttpNotFound("User not found");
            }

            ViewBag.Vehicles = new SelectList(db.Vehicles.Where(v => v.IsAvailable), "Id", "Title");
            ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" });

            var model = new AssignVehicleViewModel
            {
                UserId = userId,
                PurchaseDate = DateTime.Today,
                NextServiceDate = DateTime.Today.AddMonths(1)
            };

            ViewBag.UserName = user.FullName;
            return View(model);
        }

        // Update your AssignVehicle method to send email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignVehicle(AssignVehicleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Vehicles = new SelectList(db.Vehicles.Where(v => v.IsAvailable), "Id", "Title", model.VehicleId);
                ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" }, model.RecurrenceType);

                var user = db.Users.Find(model.UserId);
                ViewBag.UserName = user?.FullName;

                return View(model);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Get the selected vehicle
                    var vehicle = db.Vehicles
                        .Include(v => v.Specs)
                        .FirstOrDefault(v => v.Id == model.VehicleId);

                    if (vehicle == null)
                    {
                        ModelState.AddModelError("VehicleId", "Selected vehicle not found.");
                        ViewBag.Vehicles = new SelectList(db.Vehicles.Where(v => v.IsAvailable), "Id", "Title", model.VehicleId);
                        ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" }, model.RecurrenceType);

                        var user = db.Users.Find(model.UserId);
                        ViewBag.UserName = user?.FullName;

                        return View(model);
                    }

                    // Check if vehicle is still available
                    if (!vehicle.IsAvailable)
                    {
                        ModelState.AddModelError("VehicleId", "This vehicle is no longer available.");
                        ViewBag.Vehicles = new SelectList(db.Vehicles.Where(v => v.IsAvailable), "Id", "Title", model.VehicleId);
                        ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" }, model.RecurrenceType);

                        var user = db.Users.Find(model.UserId);
                        ViewBag.UserName = user?.FullName;

                        return View(model);
                    }

                    // Check if user already has this vehicle assigned
                    var existingAssignment = db.UserVehicles
                        .FirstOrDefault(uv => uv.UserId == model.UserId && uv.VehicleId == model.VehicleId);

                    if (existingAssignment != null)
                    {
                        ModelState.AddModelError("", "This vehicle is already assigned to this user.");
                        ViewBag.Vehicles = new SelectList(db.Vehicles.Where(v => v.IsAvailable), "Id", "Title", model.VehicleId);
                        ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" }, model.RecurrenceType);

                        var user = db.Users.Find(model.UserId);
                        ViewBag.UserName = user?.FullName;

                        return View(model);
                    }

                    // Get user details
                    var userForEmail = db.Users.Find(model.UserId);
                    if (userForEmail == null)
                    {
                        ModelState.AddModelError("", "User not found.");
                        ViewBag.Vehicles = new SelectList(db.Vehicles.Where(v => v.IsAvailable), "Id", "Title", model.VehicleId);
                        ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" }, model.RecurrenceType);
                        return View(model);
                    }

                    // Mark vehicle as unavailable
                    vehicle.IsAvailable = false;
                    db.Entry(vehicle).State = EntityState.Modified;
                    db.SaveChanges();

                    // Create UserVehicle
                    var userVehicle = new UserVehicle
                    {
                        UserId = model.UserId,
                        VehicleId = model.VehicleId,
                        PurchaseDate = model.PurchaseDate,
                        Notes = model.Notes
                    };

                    db.UserVehicles.Add(userVehicle);
                    db.SaveChanges();

                    // Verify the UserVehicle was saved correctly
                    var savedUserVehicle = db.UserVehicles
                        .Where(uv => uv.UserId == model.UserId && uv.VehicleId == model.VehicleId)
                        .OrderByDescending(uv => uv.Id)
                        .FirstOrDefault();

                    if (savedUserVehicle == null || savedUserVehicle.Id == 0)
                    {
                        throw new Exception("Failed to save UserVehicle assignment");
                    }

                    // Create ServiceSchedule
                    var serviceSchedule = new ServiceSchedule
                    {
                        UserVehicleId = savedUserVehicle.Id,
                        RecurrenceType = model.RecurrenceType,
                        NextServiceDate = model.NextServiceDate,
                        Notes = model.ServiceNotes
                    };

                    db.ServiceSchedules.Add(serviceSchedule);
                    db.SaveChanges();

                    transaction.Commit();

                    // Send assignment email
                    await _emailService.SendVehicleAssignmentAsync(userForEmail, vehicle, savedUserVehicle, serviceSchedule);

                    TempData["SuccessMessage"] = $"Vehicle '{vehicle.Title}' assigned successfully to {userForEmail.FullName}. Service schedule created and customer notified.";
                    return RedirectToAction("Details", "Users", new { id = model.UserId });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    // Get the innermost exception for better error message
                    Exception innerException = ex;
                    while (innerException.InnerException != null)
                    {
                        innerException = innerException.InnerException;
                    }

                    // Log the full error for debugging
                    System.Diagnostics.Debug.WriteLine($"ERROR in AssignVehicle: {ex.ToString()}");
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {innerException.Message}");

                    ModelState.AddModelError("", $"An error occurred while assigning the vehicle: {innerException.Message}");

                    // Repopulate dropdowns
                    ViewBag.Vehicles = new SelectList(db.Vehicles.Where(v => v.IsAvailable), "Id", "Title", model.VehicleId);
                    ViewBag.RecurrenceOptions = new SelectList(new[] { "Monthly", "Quarterly", "6 Months", "Yearly" }, model.RecurrenceType);

                    var user = db.Users.Find(model.UserId);
                    ViewBag.UserName = user?.FullName;

                    return View(model);
                }
            }
        }

    }
}
