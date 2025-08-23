using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WestendMotors.Models
{
	public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("WestendDB") { }

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

            // Alternative configuration if the above doesn't work:
            // modelBuilder.Entity<VehicleSpecs>()
            //     .HasRequired(vs => vs.Vehicle)
            //     .WithOptional(v => v.Specs)
            //     .WillCascadeOnDelete(true);

            // Configure Vehicle-VehicleImage one-to-many relationship
            modelBuilder.Entity<VehicleImage>()
                .HasRequired(vi => vi.Vehicle)
                .WithMany(v => v.Images)
                .HasForeignKey(vi => vi.VehicleId)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);

            // Configure one-to-one relationship between UserVehicle and ServiceSchedule
            modelBuilder.Entity<UserVehicle>()
                .HasOptional(uv => uv.ServiceSchedule)        // UserVehicle optionally has one ServiceSchedule
                .WithRequired(ss => ss.UserVehicle);
        }
    }
}