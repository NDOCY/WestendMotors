using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WestendMotors.Models
{
	public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("WestendDB") 
        {
            // ADD THESE LINES:
            //Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ApplicationDbContext>());
            //Database.Initialize(true); // This forces initialization
                                       // USE THIS SAFE APPROACH INSTEAD:
            Database.SetInitializer(new CreateDatabaseIfNotExists<ApplicationDbContext>());
        }


        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<TradeInRequest> TradeInRequests { get; set; }
        public DbSet<ServiceRecord> ServiceRecords { get; set; }
        public DbSet<VehicleImage> VehicleImages { get; set; }
        public DbSet<VehicleSpecs> VehicleSpec { get; set; }
        public DbSet<UserVehicle> UserVehicles { get; set; }
        public DbSet<ServiceSchedule> ServiceSchedules { get; set; }
        public DbSet<TradeInImage> TradeInImages { get; set; }
        public DbSet<TradeInAppointment> TradeInAppointments { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configure Vehicle-VehicleSpecs one-to-zero-or-one relationship
            modelBuilder.Entity<Vehicle>()
                .HasOptional(v => v.Specs)
                .WithRequired(vs => vs.Vehicle)
                .WillCascadeOnDelete(true);

            // Configure Vehicle-VehicleImage one-to-many relationship
            modelBuilder.Entity<VehicleImage>()
                .HasRequired(vi => vi.Vehicle)
                .WithMany(v => v.Images)
                .HasForeignKey(vi => vi.VehicleId)
                .WillCascadeOnDelete(true);

            // Configure User-UserVehicle one-to-many relationship
            modelBuilder.Entity<UserVehicle>()
                .HasRequired(uv => uv.User)
                .WithMany(u => u.UserVehicles)
                .HasForeignKey(uv => uv.UserId)
                .WillCascadeOnDelete(false);

            // Configure Vehicle-UserVehicle one-to-many relationship
            modelBuilder.Entity<UserVehicle>()
                .HasRequired(uv => uv.Vehicle)
                .WithMany()
                .HasForeignKey(uv => uv.VehicleId)
                .WillCascadeOnDelete(false);

            // Configure UserVehicle-ServiceSchedule one-to-many relationship
            modelBuilder.Entity<ServiceSchedule>()
                .HasRequired(ss => ss.UserVehicle)
                .WithMany(uv => uv.ServiceSchedules)
                .HasForeignKey(ss => ss.UserVehicleId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ServiceSchedule>()
        .HasKey(s => s.Id); // Explicitly define the primary key

            modelBuilder.Entity<ServiceSchedule>()
                .Property(s => s.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<ServiceSchedule>()
                .HasRequired(ss => ss.UserVehicle)
                .WithMany(uv => uv.ServiceSchedules)
                .HasForeignKey(ss => ss.UserVehicleId)
                .WillCascadeOnDelete(true);

            

            base.OnModelCreating(modelBuilder);
        }
    }
}