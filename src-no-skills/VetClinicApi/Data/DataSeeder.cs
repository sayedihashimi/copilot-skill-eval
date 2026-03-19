using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class DataSeeder
{
    public static void Seed(VetClinicDbContext context)
    {
        if (context.Owners.Any()) return;

        var now = DateTime.UtcNow;

        // Owners
        var owners = new List<Owner>
        {
            new() { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701", CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Ave", City = "Portland", State = "OR", ZipCode = "97201", CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Blvd", City = "Austin", State = "TX", ZipCode = "73301", CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, FirstName = "James", LastName = "Wilson", Email = "james.wilson@email.com", Phone = "555-0104", Address = "321 Elm Drive", City = "Denver", State = "CO", ZipCode = "80201", CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, FirstName = "Lisa", LastName = "Thompson", Email = "lisa.thompson@email.com", Phone = "555-0105", Address = "654 Cedar Lane", City = "Seattle", State = "WA", ZipCode = "98101", CreatedAt = now, UpdatedAt = now }
        };
        context.Owners.AddRange(owners);
        context.SaveChanges();

        // Pets
        var pets = new List<Pet>
        {
            new() { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC001", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.5m, Color = "Cream", MicrochipNumber = "MC002", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC003", IsActive = true, OwnerId = 2, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 5), Weight = 6.2m, Color = "Tabby", MicrochipNumber = "MC004", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Labrador Retriever", DateOfBirth = new DateOnly(2020, 11, 30), Weight = 29.0m, Color = "Chocolate", MicrochipNumber = "MC005", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Name = "Tweety", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.1m, Color = "Yellow", MicrochipNumber = null, IsActive = true, OwnerId = 4, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Name = "Rocky", Species = "Dog", Breed = "Bulldog", DateOfBirth = new DateOnly(2018, 8, 20), Weight = 25.0m, Color = "White and Brown", MicrochipNumber = "MC007", IsActive = true, OwnerId = 5, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Name = "Nibbles", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 1), Weight = 1.8m, Color = "Gray", MicrochipNumber = null, IsActive = true, OwnerId = 4, CreatedAt = now, UpdatedAt = now }
        };
        context.Pets.AddRange(pets);
        context.SaveChanges();

        // Veterinarians
        var vets = new List<Veterinarian>
        {
            new() { Id = 1, FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", IsAvailable = true, HireDate = new DateOnly(2015, 6, 1) },
            new() { Id = 2, FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", IsAvailable = true, HireDate = new DateOnly(2018, 3, 15) },
            new() { Id = 3, FirstName = "Dr. Patricia", LastName = "Nguyen", Email = "patricia.nguyen@happypaws.com", Phone = "555-0203", Specialization = "Dentistry", LicenseNumber = "VET-2020-003", IsAvailable = true, HireDate = new DateOnly(2020, 9, 1) }
        };
        context.Veterinarians.AddRange(vets);
        context.SaveChanges();

        var pastDate1 = now.AddDays(-30);
        var pastDate2 = now.AddDays(-20);
        var pastDate3 = now.AddDays(-10);
        var pastDate4 = now.AddDays(-5);
        var futureDate1 = now.AddDays(3);
        var futureDate2 = now.AddDays(7);
        var futureDate3 = now.AddDays(14);

        // Appointments
        var appointments = new List<Appointment>
        {
            new() { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = pastDate1, DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup and vaccination", CreatedAt = pastDate1.AddDays(-7), UpdatedAt = pastDate1 },
            new() { Id = 2, PetId = 3, VeterinarianId = 2, AppointmentDate = pastDate2, DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on front left leg", CreatedAt = pastDate2.AddDays(-5), UpdatedAt = pastDate2 },
            new() { Id = 3, PetId = 2, VeterinarianId = 1, AppointmentDate = pastDate3, DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", CreatedAt = pastDate3.AddDays(-3), UpdatedAt = pastDate3 },
            new() { Id = 4, PetId = 5, VeterinarianId = 3, AppointmentDate = pastDate4, DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Skin irritation and itching", CreatedAt = pastDate4.AddDays(-3), UpdatedAt = pastDate4 },
            new() { Id = 5, PetId = 4, VeterinarianId = 1, AppointmentDate = pastDate4.AddHours(2), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Routine vaccination", CancellationReason = "Owner had a scheduling conflict", CreatedAt = pastDate4.AddDays(-5), UpdatedAt = pastDate4 },
            new() { Id = 6, PetId = 1, VeterinarianId = 1, AppointmentDate = futureDate1.Date.AddHours(9), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up on vaccination", CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, PetId = 7, VeterinarianId = 2, AppointmentDate = futureDate2.Date.AddHours(10), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Hip joint evaluation", CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, PetId = 6, VeterinarianId = 3, AppointmentDate = futureDate2.Date.AddHours(14), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Wing feather trimming", CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, PetId = 3, VeterinarianId = 1, AppointmentDate = futureDate3.Date.AddHours(11), DurationMinutes = 60, Status = AppointmentStatus.Scheduled, Reason = "Post-surgery follow-up", CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, PetId = 8, VeterinarianId = 3, AppointmentDate = futureDate3.Date.AddHours(15), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Nail trimming and general checkup", CreatedAt = now, UpdatedAt = now }
        };
        context.Appointments.AddRange(appointments);
        context.SaveChanges();

        // Medical Records (for completed appointments)
        var records = new List<MedicalRecord>
        {
            new() { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy. All vitals normal.", Treatment = "Administered annual DHPP and Rabies vaccines. Heartworm test negative.", Notes = "Weight is ideal. Continue current diet.", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(12)), CreatedAt = pastDate1 },
            new() { Id = 2, AppointmentId = 2, PetId = 3, VeterinarianId = 2, Diagnosis = "Mild sprain in the left front leg", Treatment = "Anti-inflammatory medication prescribed. Rest recommended for 2 weeks. Cold compress application advised.", Notes = "X-ray showed no fracture. Monitor for improvement.", FollowUpDate = DateOnly.FromDateTime(now.AddDays(14)), CreatedAt = pastDate2 },
            new() { Id = 3, AppointmentId = 3, PetId = 2, VeterinarianId = 1, Diagnosis = "Mild gingivitis and tartar buildup", Treatment = "Full dental cleaning performed under sedation. One tooth extraction (upper premolar). Antibiotics prescribed.", Notes = "Cat recovered well from anesthesia. Feed soft food for 5 days.", CreatedAt = pastDate3 },
            new() { Id = 4, AppointmentId = 4, PetId = 5, VeterinarianId = 3, Diagnosis = "Allergic dermatitis, likely environmental", Treatment = "Prescribed antihistamines and medicated shampoo. Recommended hypoallergenic diet trial.", Notes = "Avoid walks during high pollen days. Return if no improvement in 2 weeks.", FollowUpDate = DateOnly.FromDateTime(now.AddDays(14)), CreatedAt = pastDate4 }
        };
        context.MedicalRecords.AddRange(records);
        context.SaveChanges();

        // Prescriptions
        var prescriptions = new List<Prescription>
        {
            new() { Id = 1, MedicalRecordId = 2, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = DateOnly.FromDateTime(pastDate2), EndDate = DateOnly.FromDateTime(pastDate2).AddDays(14), Instructions = "Give with food. Monitor for GI upset.", CreatedAt = pastDate2 },
            new() { Id = 2, MedicalRecordId = 3, MedicationName = "Amoxicillin", Dosage = "50mg twice daily", DurationDays = 10, StartDate = DateOnly.FromDateTime(pastDate3), EndDate = DateOnly.FromDateTime(pastDate3).AddDays(10), Instructions = "Complete full course even if symptoms improve.", CreatedAt = pastDate3 },
            new() { Id = 3, MedicalRecordId = 4, MedicationName = "Diphenhydramine", Dosage = "25mg once daily", DurationDays = 30, StartDate = DateOnly.FromDateTime(pastDate4), EndDate = DateOnly.FromDateTime(pastDate4).AddDays(30), Instructions = "Give in the evening. May cause drowsiness.", CreatedAt = pastDate4 },
            new() { Id = 4, MedicalRecordId = 4, MedicationName = "Chlorhexidine Shampoo", Dosage = "Apply twice weekly", DurationDays = 28, StartDate = DateOnly.FromDateTime(pastDate4), EndDate = DateOnly.FromDateTime(pastDate4).AddDays(28), Instructions = "Lather and leave on for 10 minutes before rinsing.", CreatedAt = pastDate4 },
            new() { Id = 5, MedicalRecordId = 1, MedicationName = "Heartgard Plus", Dosage = "One chewable monthly", DurationDays = 365, StartDate = DateOnly.FromDateTime(pastDate1), EndDate = DateOnly.FromDateTime(pastDate1).AddDays(365), Instructions = "Give on the same day each month. Heartworm prevention.", CreatedAt = pastDate1 }
        };
        context.Prescriptions.AddRange(prescriptions);
        context.SaveChanges();

        // Vaccinations
        var vaccinations = new List<Vaccination>
        {
            new() { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(pastDate1), ExpirationDate = DateOnly.FromDateTime(pastDate1).AddYears(3), BatchNumber = "RAB-2024-001", AdministeredByVetId = 1, Notes = "3-year rabies vaccine", CreatedAt = pastDate1 },
            new() { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = DateOnly.FromDateTime(pastDate1), ExpirationDate = DateOnly.FromDateTime(pastDate1).AddYears(1), BatchNumber = "DHPP-2024-042", AdministeredByVetId = 1, Notes = "Annual booster", CreatedAt = pastDate1 },
            new() { Id = 3, PetId = 3, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-11)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(25)), BatchNumber = "RAB-2024-015", AdministeredByVetId = 2, Notes = "Due for renewal soon", CreatedAt = now.AddMonths(-11) },
            new() { Id = 4, PetId = 2, VaccineName = "FVRCP", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-14)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(-30)), BatchNumber = "FVRCP-2023-008", AdministeredByVetId = 1, Notes = "Overdue for renewal", CreatedAt = now.AddMonths(-14) },
            new() { Id = 5, PetId = 5, VaccineName = "Bordetella", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-6)), ExpirationDate = DateOnly.FromDateTime(now.AddMonths(6)), BatchNumber = "BOR-2024-022", AdministeredByVetId = 3, Notes = "Kennel cough vaccine", CreatedAt = now.AddMonths(-6) },
            new() { Id = 6, PetId = 7, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-2)), ExpirationDate = DateOnly.FromDateTime(now.AddMonths(10)), BatchNumber = "RAB-2024-033", AdministeredByVetId = 2, Notes = "Annual rabies", CreatedAt = now.AddMonths(-2) }
        };
        context.Vaccinations.AddRange(vaccinations);
        context.SaveChanges();
    }
}
