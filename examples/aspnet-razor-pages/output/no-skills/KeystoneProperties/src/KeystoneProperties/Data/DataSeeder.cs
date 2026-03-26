using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;

namespace KeystoneProperties.Data;

public static class DataSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Properties.Any()) return;

        // Properties
        var properties = new List<Property>
        {
            new() { Id = 1, Name = "Maple Ridge Apartments", Address = "1200 Maple Ridge Dr", City = "Springfield", State = "IL", ZipCode = "62704", PropertyType = PropertyType.Apartment, YearBuilt = 2005, TotalUnits = 12, Description = "Modern apartment complex with pool and fitness center", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-24), UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Cedar Park Townhomes", Address = "450 Cedar Park Ln", City = "Springfield", State = "IL", ZipCode = "62711", PropertyType = PropertyType.Townhouse, YearBuilt = 2010, TotalUnits = 8, Description = "Spacious townhomes in quiet neighborhood near parks", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-18), UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, Name = "Lakeview Condos", Address = "800 Lakeview Blvd", City = "Chatham", State = "IL", ZipCode = "62629", PropertyType = PropertyType.Condo, YearBuilt = 2015, TotalUnits = 6, Description = "Luxury condos overlooking Lake Springfield", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-12), UpdatedAt = DateTime.UtcNow },
        };
        context.Properties.AddRange(properties);
        context.SaveChanges();

        // Units - Maple Ridge (12 units)
        var units = new List<Unit>
        {
            new() { Id = 1, PropertyId = 1, UnitNumber = "101", Floor = 1, Bedrooms = 1, Bathrooms = 1, SquareFeet = 650, MonthlyRent = 950, DepositAmount = 950, Status = UnitStatus.Occupied, Amenities = "Washer/Dryer hookup, Patio" },
            new() { Id = 2, PropertyId = 1, UnitNumber = "102", Floor = 1, Bedrooms = 2, Bathrooms = 1, SquareFeet = 850, MonthlyRent = 1200, DepositAmount = 1200, Status = UnitStatus.Occupied, Amenities = "Washer/Dryer, Patio, Storage" },
            new() { Id = 3, PropertyId = 1, UnitNumber = "103", Floor = 1, Bedrooms = 0, Bathrooms = 1, SquareFeet = 450, MonthlyRent = 800, DepositAmount = 800, Status = UnitStatus.Available, Amenities = "Kitchenette" },
            new() { Id = 4, PropertyId = 1, UnitNumber = "201", Floor = 2, Bedrooms = 2, Bathrooms = 2, SquareFeet = 950, MonthlyRent = 1400, DepositAmount = 1400, Status = UnitStatus.Occupied, Amenities = "Washer/Dryer, Balcony, Walk-in Closet" },
            new() { Id = 5, PropertyId = 1, UnitNumber = "202", Floor = 2, Bedrooms = 1, Bathrooms = 1, SquareFeet = 650, MonthlyRent = 975, DepositAmount = 975, Status = UnitStatus.Occupied, Amenities = "Balcony" },
            new() { Id = 6, PropertyId = 1, UnitNumber = "203", Floor = 2, Bedrooms = 3, Bathrooms = 2, SquareFeet = 1200, MonthlyRent = 1650, DepositAmount = 1650, Status = UnitStatus.Occupied, Amenities = "Washer/Dryer, Balcony, Walk-in Closet, Parking" },
            // Cedar Park (8 units)
            new() { Id = 7, PropertyId = 2, UnitNumber = "A1", Floor = 1, Bedrooms = 2, Bathrooms = 1.5m, SquareFeet = 1100, MonthlyRent = 1350, DepositAmount = 1350, Status = UnitStatus.Occupied, Amenities = "Garage, Yard, Washer/Dryer" },
            new() { Id = 8, PropertyId = 2, UnitNumber = "A2", Floor = 1, Bedrooms = 3, Bathrooms = 2.5m, SquareFeet = 1500, MonthlyRent = 1800, DepositAmount = 1800, Status = UnitStatus.Occupied, Amenities = "Garage, Yard, Washer/Dryer, Fireplace" },
            new() { Id = 9, PropertyId = 2, UnitNumber = "B1", Floor = 1, Bedrooms = 2, Bathrooms = 1.5m, SquareFeet = 1100, MonthlyRent = 1350, DepositAmount = 1350, Status = UnitStatus.Available, Amenities = "Garage, Yard, Washer/Dryer" },
            new() { Id = 10, PropertyId = 2, UnitNumber = "B2", Floor = 1, Bedrooms = 3, Bathrooms = 2, SquareFeet = 1400, MonthlyRent = 1750, DepositAmount = 1750, Status = UnitStatus.Maintenance, Amenities = "Garage, Yard, Washer/Dryer" },
            // Lakeview Condos (6 units)
            new() { Id = 11, PropertyId = 3, UnitNumber = "1A", Floor = 1, Bedrooms = 2, Bathrooms = 2, SquareFeet = 1050, MonthlyRent = 1500, DepositAmount = 1500, Status = UnitStatus.Occupied, Amenities = "Lake View, Balcony, Parking, Pool Access" },
            new() { Id = 12, PropertyId = 3, UnitNumber = "1B", Floor = 1, Bedrooms = 1, Bathrooms = 1, SquareFeet = 750, MonthlyRent = 1100, DepositAmount = 1100, Status = UnitStatus.Occupied, Amenities = "Parking, Pool Access" },
            new() { Id = 13, PropertyId = 3, UnitNumber = "2A", Floor = 2, Bedrooms = 3, Bathrooms = 2, SquareFeet = 1300, MonthlyRent = 2000, DepositAmount = 2000, Status = UnitStatus.Available, Amenities = "Lake View, Balcony, Parking, Pool Access, Fireplace" },
            new() { Id = 14, PropertyId = 3, UnitNumber = "2B", Floor = 2, Bedrooms = 2, Bathrooms = 2, SquareFeet = 1050, MonthlyRent = 1600, DepositAmount = 1600, Status = UnitStatus.Occupied, Amenities = "Lake View, Balcony, Parking, Pool Access" },
            new() { Id = 15, PropertyId = 3, UnitNumber = "3A", Floor = 3, Bedrooms = 4, Bathrooms = 3, SquareFeet = 1800, MonthlyRent = 2200, DepositAmount = 2200, Status = UnitStatus.Available, Amenities = "Penthouse, Lake View, Balcony, Parking, Pool Access" },
        };
        context.Units.AddRange(units);
        context.SaveChanges();

        // Tenants
        var tenants = new List<Tenant>
        {
            new() { Id = 1, FirstName = "James", LastName = "Wilson", Email = "james.wilson@email.com", Phone = "217-555-0101", DateOfBirth = new DateOnly(1988, 3, 15), EmergencyContactName = "Sarah Wilson", EmergencyContactPhone = "217-555-0102" },
            new() { Id = 2, FirstName = "Maria", LastName = "Garcia", Email = "maria.garcia@email.com", Phone = "217-555-0201", DateOfBirth = new DateOnly(1992, 7, 22), EmergencyContactName = "Carlos Garcia", EmergencyContactPhone = "217-555-0202" },
            new() { Id = 3, FirstName = "David", LastName = "Chen", Email = "david.chen@email.com", Phone = "217-555-0301", DateOfBirth = new DateOnly(1985, 11, 8), EmergencyContactName = "Wei Chen", EmergencyContactPhone = "217-555-0302" },
            new() { Id = 4, FirstName = "Emily", LastName = "Johnson", Email = "emily.johnson@email.com", Phone = "217-555-0401", DateOfBirth = new DateOnly(1995, 1, 30), EmergencyContactName = "Robert Johnson", EmergencyContactPhone = "217-555-0402" },
            new() { Id = 5, FirstName = "Michael", LastName = "Brown", Email = "michael.brown@email.com", Phone = "217-555-0501", DateOfBirth = new DateOnly(1990, 5, 12), EmergencyContactName = "Lisa Brown", EmergencyContactPhone = "217-555-0502" },
            new() { Id = 6, FirstName = "Sarah", LastName = "Davis", Email = "sarah.davis@email.com", Phone = "217-555-0601", DateOfBirth = new DateOnly(1987, 9, 25), EmergencyContactName = "Tom Davis", EmergencyContactPhone = "217-555-0602" },
            new() { Id = 7, FirstName = "Kevin", LastName = "Martinez", Email = "kevin.martinez@email.com", Phone = "217-555-0701", DateOfBirth = new DateOnly(1993, 4, 18), EmergencyContactName = "Rosa Martinez", EmergencyContactPhone = "217-555-0702" },
            new() { Id = 8, FirstName = "Jennifer", LastName = "Taylor", Email = "jennifer.taylor@email.com", Phone = "217-555-0801", DateOfBirth = new DateOnly(1991, 12, 3), EmergencyContactName = "Mark Taylor", EmergencyContactPhone = "217-555-0802" },
            new() { Id = 9, FirstName = "Robert", LastName = "Anderson", Email = "robert.anderson@email.com", Phone = "217-555-0901", DateOfBirth = new DateOnly(1986, 8, 14), EmergencyContactName = "Nancy Anderson", EmergencyContactPhone = "217-555-0902" },
            new() { Id = 10, FirstName = "Amanda", LastName = "Thomas", Email = "amanda.thomas@email.com", Phone = "217-555-1001", DateOfBirth = new DateOnly(1994, 6, 7), EmergencyContactName = "Chris Thomas", EmergencyContactPhone = "217-555-1002", IsActive = false },
        };
        context.Tenants.AddRange(tenants);
        context.SaveChanges();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Leases
        var leases = new List<Lease>
        {
            // Active leases
            new() { Id = 1, UnitId = 1, TenantId = 1, StartDate = today.AddMonths(-6), EndDate = today.AddMonths(6), MonthlyRentAmount = 950, DepositAmount = 950, Status = LeaseStatus.Active },
            new() { Id = 2, UnitId = 2, TenantId = 2, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(9), MonthlyRentAmount = 1200, DepositAmount = 1200, Status = LeaseStatus.Active },
            new() { Id = 3, UnitId = 4, TenantId = 3, StartDate = today.AddMonths(-8), EndDate = today.AddMonths(4), MonthlyRentAmount = 1400, DepositAmount = 1400, Status = LeaseStatus.Active },
            new() { Id = 4, UnitId = 5, TenantId = 4, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(10), MonthlyRentAmount = 975, DepositAmount = 975, Status = LeaseStatus.Active },
            new() { Id = 5, UnitId = 6, TenantId = 5, StartDate = today.AddMonths(-10), EndDate = today.AddMonths(2), MonthlyRentAmount = 1650, DepositAmount = 1650, Status = LeaseStatus.Active },
            new() { Id = 6, UnitId = 7, TenantId = 6, StartDate = today.AddMonths(-4), EndDate = today.AddMonths(8), MonthlyRentAmount = 1350, DepositAmount = 1350, Status = LeaseStatus.Active },
            new() { Id = 7, UnitId = 8, TenantId = 7, StartDate = today.AddMonths(-5), EndDate = today.AddMonths(7), MonthlyRentAmount = 1800, DepositAmount = 1800, Status = LeaseStatus.Active },
            new() { Id = 8, UnitId = 11, TenantId = 8, StartDate = today.AddMonths(-7), EndDate = today.AddMonths(5), MonthlyRentAmount = 1500, DepositAmount = 1500, Status = LeaseStatus.Active },
            new() { Id = 9, UnitId = 12, TenantId = 9, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(11), MonthlyRentAmount = 1100, DepositAmount = 1100, Status = LeaseStatus.Active },
            new() { Id = 10, UnitId = 14, TenantId = 5, StartDate = today.AddMonths(-9), EndDate = today.AddDays(15), MonthlyRentAmount = 1600, DepositAmount = 1600, Status = LeaseStatus.Active },
            // Expired lease (renewal chain: 11 -> 1 renewed it)
            new() { Id = 11, UnitId = 1, TenantId = 1, StartDate = today.AddMonths(-18), EndDate = today.AddMonths(-6).AddDays(-1), MonthlyRentAmount = 900, DepositAmount = 950, Status = LeaseStatus.Renewed, DepositStatus = DepositStatus.Held },
            // Terminated lease
            new() { Id = 12, UnitId = 3, TenantId = 10, StartDate = today.AddMonths(-12), EndDate = today.AddMonths(-2), MonthlyRentAmount = 800, DepositAmount = 800, Status = LeaseStatus.Terminated, TerminationDate = today.AddMonths(-4), TerminationReason = "Tenant relocated for work. Early termination agreed.", DepositStatus = DepositStatus.PartiallyReturned },
            // Pending lease
            new() { Id = 13, UnitId = 13, TenantId = 4, StartDate = today.AddDays(15), EndDate = today.AddMonths(12).AddDays(15), MonthlyRentAmount = 2000, DepositAmount = 2000, Status = LeaseStatus.Pending },
        };
        // Set renewal chain: lease 1 is a renewal of lease 11
        leases[0].RenewalOfLeaseId = 11;
        context.Leases.AddRange(leases);
        context.SaveChanges();

        // Payments
        var payments = new List<Payment>();
        int paymentId = 1;

        // Generate rent payments for active leases
        void AddRentPayments(int leaseId, decimal amount, DateOnly leaseStart, int months, bool addLate = false)
        {
            for (int i = 0; i < months; i++)
            {
                var dueDate = leaseStart.AddMonths(i);
                var paymentDate = dueDate; // On time by default
                bool isLate = false;

                if (addLate && i == months - 2)
                {
                    paymentDate = dueDate.AddDays(8); // 8 days late
                    isLate = true;
                }

                payments.Add(new Payment
                {
                    Id = paymentId++,
                    LeaseId = leaseId,
                    Amount = amount,
                    PaymentDate = paymentDate,
                    DueDate = dueDate,
                    PaymentMethod = PaymentMethod.BankTransfer,
                    PaymentType = PaymentType.Rent,
                    Status = PaymentStatus.Completed,
                    ReferenceNumber = $"PAY-{paymentId:D5}"
                });

                if (isLate)
                {
                    int daysLate = paymentDate.DayNumber - dueDate.DayNumber;
                    decimal lateFee = Math.Min(50m + (daysLate - 5) * 5m, 200m);
                    payments.Add(new Payment
                    {
                        Id = paymentId++,
                        LeaseId = leaseId,
                        Amount = lateFee,
                        PaymentDate = paymentDate,
                        DueDate = dueDate,
                        PaymentMethod = PaymentMethod.BankTransfer,
                        PaymentType = PaymentType.LateFee,
                        Status = PaymentStatus.Completed,
                        ReferenceNumber = $"LF-{paymentId:D5}",
                        Notes = $"Late fee: {daysLate} days past due"
                    });
                }
            }
        }

        // Deposit payments
        payments.Add(new Payment { Id = paymentId++, LeaseId = 1, Amount = 950, PaymentDate = today.AddMonths(-6), DueDate = today.AddMonths(-6), PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.Deposit, Status = PaymentStatus.Completed, ReferenceNumber = "DEP-00001" });
        payments.Add(new Payment { Id = paymentId++, LeaseId = 2, Amount = 1200, PaymentDate = today.AddMonths(-3), DueDate = today.AddMonths(-3), PaymentMethod = PaymentMethod.BankTransfer, PaymentType = PaymentType.Deposit, Status = PaymentStatus.Completed, ReferenceNumber = "DEP-00002" });

        // Rent payments for first few leases
        AddRentPayments(1, 950, today.AddMonths(-6), 6, addLate: true);
        AddRentPayments(2, 1200, today.AddMonths(-3), 3);
        AddRentPayments(3, 1400, today.AddMonths(-8), 7, addLate: true);
        AddRentPayments(5, 1650, today.AddMonths(-10), 9);

        // Deposit return for terminated lease
        payments.Add(new Payment { Id = paymentId++, LeaseId = 12, Amount = 400, PaymentDate = today.AddMonths(-4), DueDate = today.AddMonths(-4), PaymentMethod = PaymentMethod.Check, PaymentType = PaymentType.DepositReturn, Status = PaymentStatus.Completed, ReferenceNumber = "DEPRET-00001", Notes = "Partial deposit return; $400 withheld for damages" });

        context.Payments.AddRange(payments);
        context.SaveChanges();

        // Maintenance Requests
        var maintenanceRequests = new List<MaintenanceRequest>
        {
            new() { Id = 1, UnitId = 1, TenantId = 1, Title = "Leaking kitchen faucet", Description = "The kitchen faucet has been dripping steadily for 2 days. Water pools under the sink.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Completed, Category = MaintenanceCategory.Plumbing, AssignedTo = "Mike's Plumbing", SubmittedDate = DateTime.UtcNow.AddDays(-14), AssignedDate = DateTime.UtcNow.AddDays(-13), CompletedDate = DateTime.UtcNow.AddDays(-11), CompletionNotes = "Replaced faucet washer and tightened connection.", EstimatedCost = 150, ActualCost = 120 },
            new() { Id = 2, UnitId = 4, TenantId = 3, Title = "AC not cooling", Description = "Air conditioning unit runs but does not cool the apartment. Temperature stays above 80°F.", Priority = MaintenancePriority.High, Status = MaintenanceStatus.InProgress, Category = MaintenanceCategory.HVAC, AssignedTo = "CoolAir Services", SubmittedDate = DateTime.UtcNow.AddDays(-3), AssignedDate = DateTime.UtcNow.AddDays(-2), EstimatedCost = 500 },
            new() { Id = 3, UnitId = 2, TenantId = 2, Title = "Broken dishwasher", Description = "Dishwasher won't start. No lights on the control panel.", Priority = MaintenancePriority.Low, Status = MaintenanceStatus.Submitted, Category = MaintenanceCategory.Appliance, SubmittedDate = DateTime.UtcNow.AddDays(-1) },
            new() { Id = 4, UnitId = 7, TenantId = 6, Title = "Garage door stuck", Description = "Garage door opener stopped working. Door is stuck in closed position.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Assigned, Category = MaintenanceCategory.General, AssignedTo = "Handy Home Repairs", SubmittedDate = DateTime.UtcNow.AddDays(-5), AssignedDate = DateTime.UtcNow.AddDays(-4), EstimatedCost = 300 },
            new() { Id = 5, UnitId = 10, TenantId = 5, Title = "Burst pipe in bathroom", Description = "Pipe burst under bathroom sink. Water flooding the floor. URGENT!", Priority = MaintenancePriority.Emergency, Status = MaintenanceStatus.InProgress, Category = MaintenanceCategory.Plumbing, AssignedTo = "Emergency Plumbing Co.", SubmittedDate = DateTime.UtcNow.AddDays(-1), AssignedDate = DateTime.UtcNow.AddDays(-1), EstimatedCost = 800 },
            new() { Id = 6, UnitId = 11, TenantId = 8, Title = "Light fixture flickering", Description = "Living room ceiling light flickers intermittently. Bulb replacement didn't fix it.", Priority = MaintenancePriority.Low, Status = MaintenanceStatus.Completed, Category = MaintenanceCategory.Electrical, AssignedTo = "Spark Electric", SubmittedDate = DateTime.UtcNow.AddDays(-21), AssignedDate = DateTime.UtcNow.AddDays(-20), CompletedDate = DateTime.UtcNow.AddDays(-18), CompletionNotes = "Replaced light switch and wiring connector.", EstimatedCost = 100, ActualCost = 85 },
            new() { Id = 7, UnitId = 6, TenantId = 5, Title = "Ants in kitchen", Description = "Finding ants along the kitchen baseboards and near the trash area.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.Cancelled, Category = MaintenanceCategory.Pest, AssignedTo = "BugBusters Pest Control", SubmittedDate = DateTime.UtcNow.AddDays(-10), AssignedDate = DateTime.UtcNow.AddDays(-9) },
            new() { Id = 8, UnitId = 8, TenantId = 7, Title = "Crack in bedroom wall", Description = "Noticed a hairline crack developing in the master bedroom wall near the window.", Priority = MaintenancePriority.Low, Status = MaintenanceStatus.Submitted, Category = MaintenanceCategory.Structural, SubmittedDate = DateTime.UtcNow.AddDays(-2) },
        };
        context.MaintenanceRequests.AddRange(maintenanceRequests);
        context.SaveChanges();

        // Inspections
        var inspections = new List<Inspection>
        {
            new() { Id = 1, UnitId = 1, InspectionType = InspectionType.MoveIn, ScheduledDate = today.AddMonths(-6), CompletedDate = today.AddMonths(-6), InspectorName = "John Harris", OverallCondition = OverallCondition.Good, Notes = "Unit in good condition. Minor scuff marks on hallway wall noted. All appliances functional.", FollowUpRequired = false, LeaseId = 1 },
            new() { Id = 2, UnitId = 3, InspectionType = InspectionType.MoveOut, ScheduledDate = today.AddMonths(-4), CompletedDate = today.AddMonths(-4), InspectorName = "John Harris", OverallCondition = OverallCondition.Fair, Notes = "Carpet staining in living room. Small hole in bedroom wall. Kitchen cleaned but grease buildup on stove.", FollowUpRequired = true, LeaseId = 12 },
            new() { Id = 3, UnitId = 4, InspectionType = InspectionType.Routine, ScheduledDate = today.AddDays(-30), CompletedDate = today.AddDays(-30), InspectorName = "Maria Santos", OverallCondition = OverallCondition.Excellent, Notes = "Tenant maintains unit very well. No issues found.", FollowUpRequired = false },
            new() { Id = 4, UnitId = 11, InspectionType = InspectionType.Routine, ScheduledDate = today.AddDays(14), InspectorName = "John Harris", Notes = "Scheduled quarterly inspection.", FollowUpRequired = false },
            new() { Id = 5, UnitId = 13, InspectionType = InspectionType.MoveIn, ScheduledDate = today.AddDays(15), InspectorName = "Maria Santos", Notes = "Pre-move-in inspection for new tenant.", FollowUpRequired = false, LeaseId = 13 },
        };
        context.Inspections.AddRange(inspections);
        context.SaveChanges();
    }
}
