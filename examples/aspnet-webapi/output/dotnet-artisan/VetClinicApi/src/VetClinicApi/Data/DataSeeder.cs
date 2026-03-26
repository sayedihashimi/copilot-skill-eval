using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(VetClinicDbContext db)
    {
        if (db.Owners.Any())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Owners
        var owners = new List<Owner>
        {
            new() { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701", CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Avenue", City = "Portland", State = "OR", ZipCode = "97201", CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301", CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0104", Address = "321 Elm Boulevard", City = "Denver", State = "CO", ZipCode = "80201", CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Lisa", LastName = "Thompson", Email = "lisa.thompson@email.com", Phone = "555-0105", Address = "654 Cedar Lane", City = "Seattle", State = "WA", ZipCode = "98101", CreatedAt = now, UpdatedAt = now },
        };
        db.Owners.AddRange(owners);

        // Pets
        var pets = new List<Pet>
        {
            new() { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.8m, Color = "Cream with brown points", MicrochipNumber = "MC-002-2019", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", IsActive = true, OwnerId = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 8), Weight = 6.2m, Color = "Tabby", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2020, 11, 30), Weight = 12.5m, Color = "Tricolor", MicrochipNumber = "MC-005-2020", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Name = "Tweety", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.09m, Color = "Yellow and Gray", IsActive = true, OwnerId = 4, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Name = "Bella", Species = "Dog", Breed = "Labrador Retriever", DateOfBirth = new DateOnly(2018, 9, 5), Weight = 28.0m, Color = "Chocolate", MicrochipNumber = "MC-007-2018", IsActive = true, OwnerId = 5, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Name = "Nibbles", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 20), Weight = 1.8m, Color = "White and Brown", IsActive = true, OwnerId = 4, CreatedAt = now, UpdatedAt = now },
        };
        db.Pets.AddRange(pets);

        // Veterinarians
        var vets = new List<Veterinarian>
        {
            new() { Id = 1, FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", IsAvailable = true, HireDate = new DateOnly(2015, 6, 1) },
            new() { Id = 2, FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", IsAvailable = true, HireDate = new DateOnly(2018, 3, 15) },
            new() { Id = 3, FirstName = "Dr. Maria", LastName = "Santos", Email = "maria.santos@happypaws.com", Phone = "555-0203", Specialization = "Dentistry", LicenseNumber = "VET-2020-003", IsAvailable = true, HireDate = new DateOnly(2020, 9, 1) },
        };
        db.Veterinarians.AddRange(vets);
        await db.SaveChangesAsync();

        // Appointments (mix of statuses)
        var pastDate1 = now.AddDays(-30);
        var pastDate2 = now.AddDays(-20);
        var pastDate3 = now.AddDays(-10);
        var pastDate4 = now.AddDays(-5);
        var today = now.Date.AddHours(10);
        var futureDate1 = now.AddDays(3);
        var futureDate2 = now.AddDays(7);
        var futureDate3 = now.AddDays(14);

        var appointments = new List<Appointment>
        {
            new() { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = pastDate1, DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual wellness checkup", Notes = "All vitals normal", CreatedAt = pastDate1.AddDays(-7), UpdatedAt = pastDate1 },
            new() { Id = 2, PetId = 3, VeterinarianId = 2, AppointmentDate = pastDate2, DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on right front leg", Notes = "X-ray taken, minor sprain detected", CreatedAt = pastDate2.AddDays(-5), UpdatedAt = pastDate2 },
            new() { Id = 3, PetId = 2, VeterinarianId = 1, AppointmentDate = pastDate3, DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", CreatedAt = pastDate3.AddDays(-7), UpdatedAt = pastDate3 },
            new() { Id = 4, PetId = 5, VeterinarianId = 3, AppointmentDate = pastDate4, DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Ear infection follow-up", Notes = "Infection clearing up", CreatedAt = pastDate4.AddDays(-3), UpdatedAt = pastDate4 },
            new() { Id = 5, PetId = 7, VeterinarianId = 1, AppointmentDate = pastDate4.AddHours(2), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Vaccination booster", CancellationReason = "Owner rescheduled due to conflict", CreatedAt = pastDate4.AddDays(-5), UpdatedAt = pastDate4 },
            new() { Id = 6, PetId = 4, VeterinarianId = 1, AppointmentDate = today, DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Routine checkup", CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2) },
            new() { Id = 7, PetId = 1, VeterinarianId = 2, AppointmentDate = futureDate1, DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Vaccination booster", CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1) },
            new() { Id = 8, PetId = 6, VeterinarianId = 3, AppointmentDate = futureDate2, DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Wing trimming and checkup", CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, PetId = 8, VeterinarianId = 1, AppointmentDate = futureDate3, DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Initial wellness exam", CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, PetId = 3, VeterinarianId = 2, AppointmentDate = pastDate1.AddHours(3), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Follow-up on vaccination", CreatedAt = pastDate1.AddDays(-3), UpdatedAt = pastDate1 },
        };
        db.Appointments.AddRange(appointments);
        await db.SaveChangesAsync();

        // Medical Records (for completed appointments)
        var medicalRecords = new List<MedicalRecord>
        {
            new() { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy — no issues found", Treatment = "Annual vaccinations administered (DHPP, Rabies)", Notes = "Weight stable, teeth in good condition", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(12)), CreatedAt = pastDate1 },
            new() { Id = 2, AppointmentId = 2, PetId = 3, VeterinarianId = 2, Diagnosis = "Grade 1 sprain of the right carpal joint", Treatment = "Rest for 2 weeks, anti-inflammatory medication prescribed, cold compress recommended", Notes = "Recheck in 2 weeks", FollowUpDate = DateOnly.FromDateTime(now.AddDays(-6)), CreatedAt = pastDate2 },
            new() { Id = 3, AppointmentId = 3, PetId = 2, VeterinarianId = 1, Diagnosis = "Mild tartar buildup on molars", Treatment = "Professional dental cleaning performed under light sedation", Notes = "Recommend dental treats and regular brushing", CreatedAt = pastDate3 },
            new() { Id = 4, AppointmentId = 4, PetId = 5, VeterinarianId = 3, Diagnosis = "Bilateral otitis externa — resolving", Treatment = "Continue ear drops for 5 more days, ear cleaned", Notes = "Significant improvement since last visit", CreatedAt = pastDate4 },
        };
        db.MedicalRecords.AddRange(medicalRecords);
        await db.SaveChangesAsync();

        // Prescriptions
        var prescriptions = new List<Prescription>
        {
            new() { Id = 1, MedicalRecordId = 2, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = DateOnly.FromDateTime(pastDate2), EndDate = DateOnly.FromDateTime(pastDate2.AddDays(14)), Instructions = "Give with food. Discontinue if vomiting occurs.", CreatedAt = pastDate2 },
            new() { Id = 2, MedicalRecordId = 2, MedicationName = "Tramadol", Dosage = "50mg as needed for pain", DurationDays = 7, StartDate = DateOnly.FromDateTime(pastDate2), EndDate = DateOnly.FromDateTime(pastDate2.AddDays(7)), Instructions = "Maximum twice daily. Monitor for sedation.", CreatedAt = pastDate2 },
            new() { Id = 3, MedicalRecordId = 4, MedicationName = "Otomax Ear Drops", Dosage = "5 drops in each ear twice daily", DurationDays = 10, StartDate = DateOnly.FromDateTime(pastDate4), EndDate = DateOnly.FromDateTime(pastDate4.AddDays(10)), Instructions = "Clean ears before applying drops.", CreatedAt = pastDate4 },
            new() { Id = 4, MedicalRecordId = 4, MedicationName = "Cephalexin", Dosage = "250mg twice daily", DurationDays = 14, StartDate = DateOnly.FromDateTime(pastDate4), EndDate = DateOnly.FromDateTime(pastDate4.AddDays(14)), Instructions = "Complete full course even if symptoms improve.", CreatedAt = pastDate4 },
            new() { Id = 5, MedicalRecordId = 1, MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly", DurationDays = 365, StartDate = DateOnly.FromDateTime(pastDate1), EndDate = DateOnly.FromDateTime(pastDate1.AddDays(365)), Instructions = "Give on the same day each month.", CreatedAt = pastDate1 },
        };
        db.Prescriptions.AddRange(prescriptions);

        // Vaccinations
        var vaccinations = new List<Vaccination>
        {
            new() { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(pastDate1), ExpirationDate = DateOnly.FromDateTime(now.AddYears(1)), BatchNumber = "RAB-2024-A1", AdministeredByVetId = 1, Notes = "3-year rabies vaccine", CreatedAt = pastDate1 },
            new() { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = DateOnly.FromDateTime(pastDate1), ExpirationDate = DateOnly.FromDateTime(now.AddYears(1)), BatchNumber = "DHPP-2024-B2", AdministeredByVetId = 1, CreatedAt = pastDate1 },
            new() { Id = 3, PetId = 3, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-11)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(20)), BatchNumber = "RAB-2023-C3", AdministeredByVetId = 2, Notes = "Due for renewal soon", CreatedAt = now.AddMonths(-11) },
            new() { Id = 4, PetId = 5, VaccineName = "Bordetella", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-13)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(-30)), BatchNumber = "BOR-2023-D4", AdministeredByVetId = 3, CreatedAt = now.AddMonths(-13) },
            new() { Id = 5, PetId = 2, VaccineName = "FVRCP", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-6)), ExpirationDate = DateOnly.FromDateTime(now.AddMonths(6)), BatchNumber = "FVRCP-2024-E5", AdministeredByVetId = 1, CreatedAt = now.AddMonths(-6) },
            new() { Id = 6, PetId = 7, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-24)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(-180)), BatchNumber = "RAB-2022-F6", AdministeredByVetId = 1, Notes = "Overdue for renewal", CreatedAt = now.AddMonths(-24) },
        };
        db.Vaccinations.AddRange(vaccinations);
        await db.SaveChangesAsync();
    }
}
