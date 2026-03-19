using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class DbSeeder
{
    public static void Seed(VetClinicDbContext db)
    {
        if (db.Owners.Any())
        {
            return;
        }

        var owners = new List<Owner>
        {
            new() { FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701" },
            new() { FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Ave", City = "Portland", State = "OR", ZipCode = "97201" },
            new() { FirstName = "Emily", LastName = "Davis", Email = "emily.davis@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301" },
            new() { FirstName = "James", LastName = "Wilson", Email = "james.wilson@email.com", Phone = "555-0104", Address = "321 Elm Blvd", City = "Denver", State = "CO", ZipCode = "80201" },
            new() { FirstName = "Maria", LastName = "Garcia", Email = "maria.garcia@email.com", Phone = "555-0105", Address = "654 Cedar Lane", City = "Miami", State = "FL", ZipCode = "33101" },
        };
        db.Owners.AddRange(owners);
        db.SaveChanges();

        var pets = new List<Pet>
        {
            new() { Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", OwnerId = owners[0].Id },
            new() { Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.8m, Color = "Cream", MicrochipNumber = "MC-002-2019", OwnerId = owners[0].Id },
            new() { Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", OwnerId = owners[1].Id },
            new() { Name = "Luna", Species = "Cat", Breed = "Persian", DateOfBirth = new DateOnly(2022, 5, 3), Weight = 3.9m, Color = "White", OwnerId = owners[2].Id },
            new() { Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2021, 11, 18), Weight = 12.3m, Color = "Tricolor", MicrochipNumber = "MC-005-2021", OwnerId = owners[2].Id },
            new() { Name = "Tweety", Species = "Bird", Breed = "Canary", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.025m, Color = "Yellow", OwnerId = owners[3].Id },
            new() { Name = "Coco", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 8, 1), Weight = 1.8m, Color = "Brown", OwnerId = owners[3].Id },
            new() { Name = "Bella", Species = "Dog", Breed = "Labrador Retriever", DateOfBirth = new DateOnly(2020, 9, 5), Weight = 28.0m, Color = "Chocolate", MicrochipNumber = "MC-008-2020", OwnerId = owners[4].Id },
        };
        db.Pets.AddRange(pets);
        db.SaveChanges();

        var vets = new List<Veterinarian>
        {
            new() { FirstName = "Dr. Robert", LastName = "Smith", Email = "r.smith@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", HireDate = new DateOnly(2015, 6, 1) },
            new() { FirstName = "Dr. Amanda", LastName = "Lee", Email = "a.lee@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", HireDate = new DateOnly(2018, 3, 15) },
            new() { FirstName = "Dr. Carlos", LastName = "Ramirez", Email = "c.ramirez@happypaws.com", Phone = "555-0203", Specialization = "Dermatology", LicenseNumber = "VET-2020-003", HireDate = new DateOnly(2020, 1, 10) },
        };
        db.Veterinarians.AddRange(vets);
        db.SaveChanges();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var appointments = new List<Appointment>
        {
            // Completed appointments (past)
            new() { PetId = pets[0].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-30), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup" },
            new() { PetId = pets[2].Id, VeterinarianId = vets[1].Id, AppointmentDate = now.AddDays(-20), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on left front leg" },
            new() { PetId = pets[4].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-15), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Ear infection follow-up" },
            new() { PetId = pets[7].Id, VeterinarianId = vets[2].Id, AppointmentDate = now.AddDays(-10), DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Skin rash examination" },
            // Cancelled
            new() { PetId = pets[1].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-5), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Vaccination booster", CancellationReason = "Owner had scheduling conflict" },
            // No-show
            new() { PetId = pets[3].Id, VeterinarianId = vets[2].Id, AppointmentDate = now.AddDays(-2), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Dental cleaning" },
            // Today
            new() { PetId = pets[5].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.Date.AddHours(10), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Wing feather trimming" },
            new() { PetId = pets[6].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.Date.AddHours(14), DurationMinutes = 30, Status = AppointmentStatus.CheckedIn, Reason = "Nail trimming and health check" },
            // Future
            new() { PetId = pets[0].Id, VeterinarianId = vets[1].Id, AppointmentDate = now.AddDays(7).Date.AddHours(9), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Dental cleaning procedure" },
            new() { PetId = pets[2].Id, VeterinarianId = vets[2].Id, AppointmentDate = now.AddDays(14).Date.AddHours(11), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up skin check" },
        };
        db.Appointments.AddRange(appointments);
        db.SaveChanges();

        var medicalRecords = new List<MedicalRecord>
        {
            new() { AppointmentId = appointments[0].Id, PetId = pets[0].Id, VeterinarianId = vets[0].Id, Diagnosis = "Healthy - no concerns found", Treatment = "Routine physical exam completed. All vitals normal.", Notes = "Weight is appropriate for age and breed.", FollowUpDate = today.AddMonths(12) },
            new() { AppointmentId = appointments[1].Id, PetId = pets[2].Id, VeterinarianId = vets[1].Id, Diagnosis = "Mild sprain in left front leg", Treatment = "Anti-inflammatory medication prescribed. Rest for 2 weeks.", Notes = "No fracture detected on X-ray.", FollowUpDate = today.AddDays(14) },
            new() { AppointmentId = appointments[2].Id, PetId = pets[4].Id, VeterinarianId = vets[0].Id, Diagnosis = "Ear infection - resolved", Treatment = "Ear drops continued for 3 more days. Infection clearing up well.", Notes = "Previous antibiotic course was effective." },
            new() { AppointmentId = appointments[3].Id, PetId = pets[7].Id, VeterinarianId = vets[2].Id, Diagnosis = "Allergic dermatitis", Treatment = "Antihistamine prescribed. Hypoallergenic diet recommended.", Notes = "Suspected food allergy. Elimination diet trial for 8 weeks.", FollowUpDate = today.AddDays(56) },
        };
        db.MedicalRecords.AddRange(medicalRecords);
        db.SaveChanges();

        var prescriptions = new List<Prescription>
        {
            new() { MedicalRecordId = medicalRecords[1].Id, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = today.AddDays(-20), Instructions = "Give with food" },
            new() { MedicalRecordId = medicalRecords[2].Id, MedicationName = "Otibiotic Ointment", Dosage = "3 drops in left ear twice daily", DurationDays = 7, StartDate = today.AddDays(-15), Instructions = "Clean ear before applying" },
            new() { MedicalRecordId = medicalRecords[3].Id, MedicationName = "Diphenhydramine", Dosage = "25mg once daily", DurationDays = 30, StartDate = today.AddDays(-10), Instructions = "Administer in the evening" },
            new() { MedicalRecordId = medicalRecords[3].Id, MedicationName = "Omega-3 Supplement", Dosage = "1 capsule daily", DurationDays = 60, StartDate = today.AddDays(-10), Instructions = "Mix with food" },
            new() { MedicalRecordId = medicalRecords[0].Id, MedicationName = "Heartworm Prevention", Dosage = "1 chewable monthly", DurationDays = 365, StartDate = today.AddDays(-30), Instructions = "Give on the same day each month" },
        };
        db.Prescriptions.AddRange(prescriptions);
        db.SaveChanges();

        var vaccinations = new List<Vaccination>
        {
            // Current
            new() { PetId = pets[0].Id, VaccineName = "Rabies", DateAdministered = today.AddMonths(-6), ExpirationDate = today.AddMonths(6), BatchNumber = "RAB-2025-A1", AdministeredByVetId = vets[0].Id },
            new() { PetId = pets[0].Id, VaccineName = "DHPP", DateAdministered = today.AddMonths(-3), ExpirationDate = today.AddMonths(9), BatchNumber = "DHPP-2025-B2", AdministeredByVetId = vets[0].Id },
            // Due soon (within 30 days)
            new() { PetId = pets[2].Id, VaccineName = "Rabies", DateAdministered = today.AddMonths(-11), ExpirationDate = today.AddDays(20), BatchNumber = "RAB-2024-C3", AdministeredByVetId = vets[1].Id },
            // Expired
            new() { PetId = pets[4].Id, VaccineName = "Bordetella", DateAdministered = today.AddMonths(-14), ExpirationDate = today.AddDays(-30), BatchNumber = "BOR-2024-D4", AdministeredByVetId = vets[0].Id },
            new() { PetId = pets[7].Id, VaccineName = "DHPP", DateAdministered = today.AddMonths(-15), ExpirationDate = today.AddDays(-60), BatchNumber = "DHPP-2024-E5", AdministeredByVetId = vets[2].Id },
            // Current
            new() { PetId = pets[7].Id, VaccineName = "Rabies", DateAdministered = today.AddMonths(-2), ExpirationDate = today.AddMonths(10), BatchNumber = "RAB-2025-F6", AdministeredByVetId = vets[0].Id, Notes = "Three-year vaccine administered" },
        };
        db.Vaccinations.AddRange(vaccinations);
        db.SaveChanges();
    }
}
