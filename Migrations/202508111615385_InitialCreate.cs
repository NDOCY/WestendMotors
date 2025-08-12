namespace WestendMotors.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Appointments",
                c => new
                    {
                        AppointmentId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        VehicleId = c.Int(),
                        Title = c.String(),
                        AppointmentType = c.String(nullable: false, maxLength: 50),
                        AppointmentDate = c.DateTime(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                        Notes = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.AppointmentId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Vehicles", t => t.VehicleId)
                .Index(t => t.UserId)
                .Index(t => t.VehicleId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 150),
                        PasswordHash = c.String(nullable: false, maxLength: 255),
                        Role = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.Vehicles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 1000),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DateAdded = c.DateTime(nullable: false),
                        IsAvailable = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.VehicleImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VehicleId = c.Int(nullable: false),
                        ImagePath = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Vehicles", t => t.VehicleId, cascadeDelete: true)
                .Index(t => t.VehicleId);
            
            CreateTable(
                "dbo.VehicleSpecs",
                c => new
                    {
                        VehicleId = c.Int(nullable: false),
                        Make = c.String(nullable: false, maxLength: 50),
                        Model = c.String(nullable: false, maxLength: 50),
                        Year = c.Int(nullable: false),
                        Mileage = c.Int(nullable: false),
                        FuelType = c.String(maxLength: 30),
                        Transmission = c.String(maxLength: 30),
                        Color = c.String(maxLength: 30),
                        EngineSize = c.Double(nullable: false),
                        NumberOfSeats = c.Int(nullable: false),
                        BodyType = c.String(maxLength: 30),
                        ConditionNotes = c.String(maxLength: 500),
                        FeatureList = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.VehicleId)
                .ForeignKey("dbo.Vehicles", t => t.VehicleId, cascadeDelete: true)
                .Index(t => t.VehicleId);
            
            CreateTable(
                "dbo.ServiceRecords",
                c => new
                    {
                        ServiceRecordId = c.Int(nullable: false, identity: true),
                        VehicleId = c.Int(nullable: false),
                        ServiceDate = c.DateTime(nullable: false),
                        Description = c.String(nullable: false, maxLength: 500),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.ServiceRecordId)
                .ForeignKey("dbo.Vehicles", t => t.VehicleId, cascadeDelete: true)
                .Index(t => t.VehicleId);
            
            CreateTable(
                "dbo.TradeInRequests",
                c => new
                    {
                        TradeInRequestId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Make = c.String(nullable: false, maxLength: 50),
                        Model = c.String(nullable: false, maxLength: 50),
                        Year = c.Int(nullable: false),
                        Mileage = c.Int(nullable: false),
                        Condition = c.String(nullable: false, maxLength: 20),
                        EstimatedValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RequestDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TradeInRequestId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TradeInRequests", "UserId", "dbo.Users");
            DropForeignKey("dbo.ServiceRecords", "VehicleId", "dbo.Vehicles");
            DropForeignKey("dbo.Appointments", "VehicleId", "dbo.Vehicles");
            DropForeignKey("dbo.VehicleSpecs", "VehicleId", "dbo.Vehicles");
            DropForeignKey("dbo.VehicleImages", "VehicleId", "dbo.Vehicles");
            DropForeignKey("dbo.Appointments", "UserId", "dbo.Users");
            DropIndex("dbo.TradeInRequests", new[] { "UserId" });
            DropIndex("dbo.ServiceRecords", new[] { "VehicleId" });
            DropIndex("dbo.VehicleSpecs", new[] { "VehicleId" });
            DropIndex("dbo.VehicleImages", new[] { "VehicleId" });
            DropIndex("dbo.Appointments", new[] { "VehicleId" });
            DropIndex("dbo.Appointments", new[] { "UserId" });
            DropTable("dbo.TradeInRequests");
            DropTable("dbo.ServiceRecords");
            DropTable("dbo.VehicleSpecs");
            DropTable("dbo.VehicleImages");
            DropTable("dbo.Vehicles");
            DropTable("dbo.Users");
            DropTable("dbo.Appointments");
        }
    }
}
