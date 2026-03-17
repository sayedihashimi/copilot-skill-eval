using VetClinicApi.Models;

namespace VetClinicApi.Data;

public sealed class DataSeeder(VetClinicDbContext context, ILogger<DataSeeder> logger)
{
    private readonly VetClinicDbContext _context = context;
    private readonly ILogger<DataSeeder> _logger = logger;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Owners.Any())
        {
            _logger.LogInformation("Database already seeded, skipping");
            return;
        }

        _logger.LogInformation("Seeding database with initial data...");

        var owners = CreateOwners();
        _context.Owners.AddRange(owners);
        await _context.SaveChangesAsync(cancellationToken);

        var vets = CreateVeterinarians();
        _context.Veterinarians.AddRange(vets);
        await _context.SaveChangesAsync(cancellationToken);

        var pets = CreatePets(owners);
        _context.Pets.AddRange(pets);
        await _context.SaveChangesAsync(cancellationToken);

        var appointments = CreateAppointments(pets, vets);
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync(cancellationToken);

        var records = CreateMedicalRecords(appointments);
        _context.MedicalRecords.AddRange(records);
        await _context.SaveChangesAsync(cancellationToken);

        var prescriptions = CreatePrescriptions(records);
        _context.Prescriptions.AddRange(prescriptions);
        await _context.SaveChangesAsync(cancellationToken);

        var vaccinations = CreateVaccinations(pets, vets);
        _context.Vaccinations.AddRange(vaccinations);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Database seeded successfully");
    }

    private static List<Owner> CreateOwners() =>
    [
        new()
        {
            FirstName = "Sarah", LastName = "Johnson",
            Email = "sarah.johnson@email.com", Phone = "555-0101",
            Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701"
        },
        new()
        {
            FirstName = "Michael", LastName = "Chen",
            Email = "michael.chen@email.com", Phone = "555-0102",
            Address = "456 Maple Avenue", City = "Portland", State = "OR", ZipCode = "97201"
        },
        new()
        {
            FirstName = "Emily", LastName = "Rodriguez",
            Email = "emily.rodriguez@email.com", Phone = "555-0103",
            Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301"
        },
        new()
        {
            FirstName = "James", LastName = "Williams",
            Email = "james.williams@email.com", Phone = "555-0104",
            Address = "321 Elm Drive", City = "Denver", State = "CO", ZipCode = "80201"
        },
        new()
        {
            FirstName = "Lisa", LastName = "Thompson",
            Email = "lisa.thompson@email.com", Phone = "555-0105",
            Address = "654 Birch Lane", City = "Seattle", State = "WA", ZipCode = "98101"
        }
    ];

    private static List<Veterinarian> CreateVeterinarians() =>
    [
        new()
        {
            FirstName = "Dr. Amanda", LastName = "Foster",
            Email = "amanda.foster@happypaws.com", Phone = "555-0201",
            Specialization = "General Practice", LicenseNumber = "VET-2019-001",
            HireDate = new DateOnly(2019, 3, 15)
        },
        new()
        {
            FirstName = "Dr. Robert", LastName = "Kim",
            Email = "robert.kim@happypaws.com", Phone = "555-0202",
            Specialization = "Surgery", LicenseNumber = "VET-2020-002",
            HireDate = new DateOnly(2020, 7, 1)
        },
        new()
        {
            FirstName = "Dr. Maria", LastName = "Santos",
            Email = "maria.santos@happypaws.com", Phone = "555-0203",
            Specialization = "Dermatology", LicenseNumber = "VET-2021-003",
            HireDate = new DateOnly(2021, 1, 10)
        }
    ];

    private static List<Pet> CreatePets(List<Owner> owners) =>
    [
        new()
        {
            Name = "Buddy", Species = "Dog", Breed = "Golden Retriever",
            DateOfBirth = new DateOnly(2020, 5, 15), Weight = 32.5m, Color = "Golden",
            MicrochipNumber = "MC-001-2020", OwnerId = owners[0].Id
        },
        new()
        {
            Name = "Whiskers", Species = "Cat", Breed = "Siamese",
            DateOfBirth = new DateOnly(2019, 8, 20), Weight = 4.5m, Color = "Cream/Brown",
            MicrochipNumber = "MC-002-2019", OwnerId = owners[0].Id
        },
        new()
        {
            Name = "Max", Species = "Dog", Breed = "German Shepherd",
            DateOfBirth = new DateOnly(2021, 2, 10), Weight = 38.0m, Color = "Black/Tan",
            MicrochipNumber = "MC-003-2021", OwnerId = owners[1].Id
        },
        new()
        {
            Name = "Luna", Species = "Cat", Breed = "Persian",
            DateOfBirth = new DateOnly(2022, 1, 5), Weight = 3.8m, Color = "White",
            OwnerId = owners[2].Id
        },
        new()
        {
            Name = "Charlie", Species = "Dog", Breed = "Labrador Retriever",
            DateOfBirth = new DateOnly(2020, 11, 30), Weight = 29.0m, Color = "Chocolate",
            MicrochipNumber = "MC-005-2020", OwnerId = owners[2].Id
        },
        new()
        {
            Name = "Tweety", Species = "Bird", Breed = "Cockatiel",
            DateOfBirth = new DateOnly(2023, 3, 12), Weight = 0.09m, Color = "Yellow/Gray",
            OwnerId = owners[3].Id
        },
        new()
        {
            Name = "Bella", Species = "Dog", Breed = "Poodle",
            DateOfBirth = new DateOnly(2021, 7, 22), Weight = 5.5m, Color = "Apricot",
            MicrochipNumber = "MC-007-2021", OwnerId = owners[3].Id
        },
        new()
        {
            Name = "Thumper", Species = "Rabbit", Breed = "Holland Lop",
            DateOfBirth = new DateOnly(2023, 9, 1), Weight = 1.8m, Color = "Brown/White",
            OwnerId = owners[4].Id
        }
    ];

    private static List<Appointment> CreateAppointments(List<Pet> pets, List<Veterinarian> vets)
    {
        var now = DateTime.UtcNow;
        return
        [
            // Completed appointments (past)
            new()
            {
                PetId = pets[0].Id, VeterinarianId = vets[0].Id,
                AppointmentDate = now.AddDays(-30), DurationMinutes = 30,
                Status = AppointmentStatus.Completed, Reason = "Annual wellness exam"
            },
            new()
            {
                PetId = pets[1].Id, VeterinarianId = vets[0].Id,
                AppointmentDate = now.AddDays(-25), DurationMinutes = 30,
                Status = AppointmentStatus.Completed, Reason = "Vaccination update"
            },
            new()
            {
                PetId = pets[2].Id, VeterinarianId = vets[1].Id,
                AppointmentDate = now.AddDays(-20), DurationMinutes = 60,
                Status = AppointmentStatus.Completed, Reason = "Limping on front left leg"
            },
            new()
            {
                PetId = pets[4].Id, VeterinarianId = vets[2].Id,
                AppointmentDate = now.AddDays(-15), DurationMinutes = 30,
                Status = AppointmentStatus.Completed, Reason = "Skin allergy check"
            },
            // Cancelled appointment
            new()
            {
                PetId = pets[3].Id, VeterinarianId = vets[0].Id,
                AppointmentDate = now.AddDays(-10), DurationMinutes = 30,
                Status = AppointmentStatus.Cancelled,
                Reason = "Dental cleaning",
                CancellationReason = "Owner had a scheduling conflict"
            },
            // No-show
            new()
            {
                PetId = pets[5].Id, VeterinarianId = vets[2].Id,
                AppointmentDate = now.AddDays(-5), DurationMinutes = 30,
                Status = AppointmentStatus.NoShow,
                Reason = "Wing feather trimming"
            },
            // Scheduled future appointments
            new()
            {
                PetId = pets[0].Id, VeterinarianId = vets[0].Id,
                AppointmentDate = now.AddDays(3).Date.AddHours(9), DurationMinutes = 30,
                Status = AppointmentStatus.Scheduled,
                Reason = "Follow-up wellness check"
            },
            new()
            {
                PetId = pets[6].Id, VeterinarianId = vets[1].Id,
                AppointmentDate = now.AddDays(5).Date.AddHours(10), DurationMinutes = 45,
                Status = AppointmentStatus.Scheduled,
                Reason = "Spay surgery consultation"
            },
            new()
            {
                PetId = pets[7].Id, VeterinarianId = vets[2].Id,
                AppointmentDate = now.AddDays(7).Date.AddHours(14), DurationMinutes = 30,
                Status = AppointmentStatus.Scheduled,
                Reason = "General health checkup"
            },
            new()
            {
                PetId = pets[3].Id, VeterinarianId = vets[0].Id,
                AppointmentDate = now.AddDays(10).Date.AddHours(11), DurationMinutes = 45,
                Status = AppointmentStatus.Scheduled,
                Reason = "Dental cleaning (rescheduled)"
            }
        ];
    }

    private static List<MedicalRecord> CreateMedicalRecords(List<Appointment> appointments) =>
    [
        new()
        {
            AppointmentId = appointments[0].Id, PetId = appointments[0].PetId,
            VeterinarianId = appointments[0].VeterinarianId,
            Diagnosis = "Healthy - no concerns noted during annual exam",
            Treatment = "Routine physical examination completed. All vitals normal.",
            Notes = "Weight stable. Teeth in good condition. Recommend continued monthly heartworm prevention.",
            FollowUpDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6))
        },
        new()
        {
            AppointmentId = appointments[1].Id, PetId = appointments[1].PetId,
            VeterinarianId = appointments[1].VeterinarianId,
            Diagnosis = "Up to date on vaccinations",
            Treatment = "Administered FVRCP booster and rabies vaccination",
            Notes = "Cat tolerated vaccines well. Mild lethargy expected for 24 hours."
        },
        new()
        {
            AppointmentId = appointments[2].Id, PetId = appointments[2].PetId,
            VeterinarianId = appointments[2].VeterinarianId,
            Diagnosis = "Mild sprain in left front leg",
            Treatment = "Prescribed anti-inflammatory medication and rest for 2 weeks",
            Notes = "X-ray showed no fractures. Recommend limiting activity.",
            FollowUpDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14))
        },
        new()
        {
            AppointmentId = appointments[3].Id, PetId = appointments[3].PetId,
            VeterinarianId = appointments[3].VeterinarianId,
            Diagnosis = "Mild dermatitis - suspected environmental allergy",
            Treatment = "Prescribed antihistamine and medicated shampoo",
            Notes = "Recommend hypoallergenic diet trial for 8 weeks."
        }
    ];

    private static List<Prescription> CreatePrescriptions(List<MedicalRecord> records)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return
        [
            // Active prescriptions
            new()
            {
                MedicalRecordId = records[2].Id,
                MedicationName = "Carprofen", Dosage = "25mg twice daily",
                DurationDays = 14, StartDate = today.AddDays(-5),
                EndDate = today.AddDays(9),
                Instructions = "Give with food. Monitor for GI upset."
            },
            new()
            {
                MedicalRecordId = records[3].Id,
                MedicationName = "Cetirizine", Dosage = "10mg once daily",
                DurationDays = 30, StartDate = today.AddDays(-3),
                EndDate = today.AddDays(27),
                Instructions = "Administer in the morning with food."
            },
            new()
            {
                MedicalRecordId = records[3].Id,
                MedicationName = "Chlorhexidine Shampoo", Dosage = "Apply during bath twice weekly",
                DurationDays = 28, StartDate = today.AddDays(-3),
                EndDate = today.AddDays(25),
                Instructions = "Leave on for 10 minutes before rinsing."
            },
            // Expired prescriptions
            new()
            {
                MedicalRecordId = records[0].Id,
                MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly",
                DurationDays = 30, StartDate = today.AddDays(-60),
                EndDate = today.AddDays(-30),
                Instructions = "Give on the first of each month."
            },
            new()
            {
                MedicalRecordId = records[1].Id,
                MedicationName = "Amoxicillin", Dosage = "50mg twice daily",
                DurationDays = 10, StartDate = today.AddDays(-40),
                EndDate = today.AddDays(-30),
                Instructions = "Complete full course even if symptoms improve."
            }
        ];
    }

    private static List<Vaccination> CreateVaccinations(List<Pet> pets, List<Veterinarian> vets)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return
        [
            // Current vaccinations
            new()
            {
                PetId = pets[0].Id, VaccineName = "Rabies",
                DateAdministered = today.AddMonths(-6), ExpirationDate = today.AddMonths(6),
                BatchNumber = "RAB-2024-1001", AdministeredByVetId = vets[0].Id
            },
            new()
            {
                PetId = pets[0].Id, VaccineName = "DHPP",
                DateAdministered = today.AddMonths(-3), ExpirationDate = today.AddMonths(9),
                BatchNumber = "DHPP-2024-2001", AdministeredByVetId = vets[0].Id
            },
            // Expiring soon (within 30 days)
            new()
            {
                PetId = pets[2].Id, VaccineName = "Rabies",
                DateAdministered = today.AddYears(-1), ExpirationDate = today.AddDays(15),
                BatchNumber = "RAB-2023-3001", AdministeredByVetId = vets[1].Id,
                Notes = "Due for renewal soon"
            },
            // Expired
            new()
            {
                PetId = pets[1].Id, VaccineName = "FVRCP",
                DateAdministered = today.AddYears(-2), ExpirationDate = today.AddDays(-30),
                BatchNumber = "FVR-2022-4001", AdministeredByVetId = vets[0].Id,
                Notes = "Overdue - needs booster"
            },
            new()
            {
                PetId = pets[4].Id, VaccineName = "Bordetella",
                DateAdministered = today.AddMonths(-8), ExpirationDate = today.AddDays(-15),
                BatchNumber = "BOR-2024-5001", AdministeredByVetId = vets[2].Id
            },
            // Recent vaccination
            new()
            {
                PetId = pets[3].Id, VaccineName = "FVRCP",
                DateAdministered = today.AddDays(-7), ExpirationDate = today.AddYears(1),
                BatchNumber = "FVR-2025-6001", AdministeredByVetId = vets[2].Id,
                Notes = "First vaccination for this pet"
            }
        ];
    }
}
