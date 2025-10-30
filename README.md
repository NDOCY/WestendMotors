# 🚗 Westend Motors  
### Comprehensive Automotive Dealership Management System  

Westend Motors is a full-featured automotive dealership management platform built with **ASP.NET MVC**, **Entity Framework**, and **SQL Server**.  
The system provides end-to-end functionality for managing vehicle inventory, customer trade-ins, service scheduling, appointments, and user role-based operations — streamlining dealership workflows and improving customer experience.

---

## 🚀 Project Overview

Westend Motors was developed as a comprehensive **dealership management system** that integrates all aspects of vehicle sales, customer engagement, and service management into a unified platform.

### **Vehicle Inventory & Sales Management**
- **Vehicle Catalog:** Full inventory management with detailed specifications, multiple images, and dynamic pricing.  
- **Advanced Search & Filtering:** Customers can search by make, model, year, price range, fuel type, transmission, and body type.  
- **Vehicle Specifications:** Includes engine capacity, mileage, fuel efficiency, and key features.  
- **Image Management:** High-quality vehicle image uploads stored via **Azure Blob Storage**.  

### **Trade-In Management System**
- **Trade-In Requests:** Customers can submit trade-in requests directly from their profile.  
- **Comparison Tool:** Side-by-side comparison between trade-in vehicle and target dealership vehicle.  
- **Admin Review Dashboard:** Evaluate trade-in details, make offers, and schedule inspections.  
- **Status Tracking:** Monitors each trade-in lifecycle (Pending → Under Review → Approved/Declined → Scheduled → Converted).  

### **Customer Relationship Management (CRM)**
- **User Management:** Role-based system for Admin, Sales Staff, and Customers.  
- **Customer Profiles:** Centralized records with contact information, owned vehicles, and purchase history.  
- **Service History:** Tracks all maintenance and repair activities per vehicle.  
- **Communication Logs:** Includes email notifications and admin-to-customer notes.  

### **Service & Appointment Management**
- **Appointment Booking:** Customers can schedule service or maintenance appointments.  
- **Recurring Service Plans:** Auto-scheduling based on mileage or service intervals.  
- **Technician Assignment:** Admin assigns specific technicians to appointments.  
- **Status Tracking:** From Scheduled → In Progress → Completed, with customer notifications.  

### **Admin & Staff Management**
- **Role-Based Access:** Admins, sales staff, and customers each have dedicated dashboards and permissions.  
- **Staff Assignment:** Assign trade-ins, appointments, and vehicles to specific employees.  
- **Performance Overview:** Track workload, service completion rates, and appointment outcomes.  
- **Admin Dashboard:** Centralized analytics of dealership operations, inventory, and services.  

---

## 🧩 Core Features Summary

| Module | Key Features |
|---------|---------------|
| **Vehicle Management** | Full inventory management with vehicle details, specs, pricing, and status. |
| **Trade-In System** | Customer trade-in submission, valuation workflow, and admin approval pipeline. |
| **User Management** | Role-based authentication and access control for Admin, Sales, and Customers. |
| **Appointment Scheduling** | Service booking, technician assignment, and progress tracking. |
| **Service Management** | Maintenance records, recurring schedules, and full service history. |
| **Customer Portal** | Browse vehicles, submit trade-ins, book appointments, and view service logs. |
| **Admin Dashboard** | Consolidated view of sales, service operations, and dealership analytics. |

---

## 🛠️ Tech Stack

| Category | Technology |
|-----------|-------------|
| **Frontend** | HTML5, CSS3, Bootstrap 5, JavaScript, jQuery, Font Awesome |
| **Backend** | ASP.NET MVC 5, C#, .NET Framework 4.7.2 |
| **Database** | Microsoft SQL Server (Entity Framework Code-First) |
| **Cloud Storage** | Azure Blob Storage for images |
| **Authentication** | Session-based login with role-based authorization |
| **Email Service** | SMTP integration for automated notifications |
| **Version Control** | Git & GitHub |
| **Hosting (Academic)** | Azure App Service with Azure SQL Database |

---

## 🔐 Roles and Access Levels

| Role | Permissions |
|------|--------------|
| **Admin** | Full control: manage vehicles, users, trade-ins, appointments, and analytics. |
| **Sales Staff** | Manage trade-in evaluations, service bookings, and vehicle status updates. |
| **Customer** | Browse vehicles, submit trade-in requests, book service appointments, and view service history. |

---

## 📊 Key Business Processes

### **Trade-In Workflow**
1. Customer submits a trade-in request with vehicle details and images.  
2. Admin reviews the request and provides a preliminary valuation.  
3. Inspection appointment is scheduled if required.  
4. Admin finalizes offer and approves or declines trade-in.  
5. Approved vehicles are converted into dealership inventory.  
6. Vehicle status updated in system (Available, Sold, or Reserved).  

### **Vehicle Lifecycle Management**
- **New Inventory:** Admin adds vehicles manually or from trade-ins.  
- **Trade-In Conversion:** Customer’s approved vehicles become dealership stock.  
- **Sales Process:** Vehicle is marked as sold upon purchase.  
- **Service Tracking:** Vehicle service records maintained post-sale.  

### **Service Management**
- Customers book service appointments through portal.  
- Admin assigns technician and defines service scope.  
- System tracks appointment progress and updates service records automatically.  

---

## 💾 Database Schema Highlights

### **Core Entities**
- `Users` – Stores admin, sales staff, and customer credentials with roles.  
- `Vehicles` – Contains all inventory data with links to specifications and images.  
- `TradeInRequests` – Handles trade-in details, evaluation status, and images.  
- `Appointments` – Tracks service schedules and assigned technicians.  
- `ServiceRecords` – Logs completed services and maintenance actions.  
- `UserVehicles` – Maps customers to owned vehicles.

### **Key Relationships**
- **One-to-One:** `Vehicle` ↔ `VehicleSpecs`  
- **One-to-Many:** `Vehicle` ↔ `VehicleImages`, `User` ↔ `UserVehicles`  
- **Many-to-Many:** `TradeInRequests` ↔ `Images` and `Appointments`

---

## ⚙️ Installation & Setup

### **Prerequisites**
- .NET Framework 4.7.2  
- Microsoft SQL Server (LocalDB or full)  
- Azure Storage Account (optional, for image uploads)  
- SMTP Server (for notification emails)  

### **Setup Instructions**
1. Clone the repository:
   ```bash
   git clone https://github.com/NDOCY/WestendMotors.git
   cd WestendMotors
