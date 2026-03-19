using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(VetClinicDbContext context)
    {
        if (context.Owners.Any())
            return;

        // Owners
        var owners = new List<Owner>
        {
            new() { Id = 1, FirstName = "John", LastName = "Smith", Email = "john.smith@email.com", Phone = "555-0101", Address = "123 Oak St", City = "Springfield", State = "IL", ZipCode = "62701" },
            new() { Id = 2, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.j@email.com", Phone = "555-0102", Address = "456 Elm Ave", City = "Portland", State = "OR", ZipCode = "97201" },
            new() { Id = 3, FirstName = "Mike", LastName = "Williams", Email = "mike.w@email.com", Phone = "555-0103", Address = "789 Pine Rd", City = "Austin", State = "TX", ZipCode = "73301" },
            new() { Id = 4, FirstName = "Emily", LastName = "Brown", Email = "emily.b@email.com", Phone = "555-0104", Address = "321 Maple Dr", City = "Denver", State = "CO", ZipCode = "80201" },
            new() { Id = 5, FirstName = "David", LastName = "Garcia", Email = "david.g@email.com", Phone = "555-0105", Address = "654 Cedar Ln", City = "Seattle", State = "WA", ZipCode = "98101" },
        };
        context.Owners.AddRange(owners);

        // Pets
        var pets = new List<Pet>
        {
            new() { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 30.5m, Color = "Golden", MicrochipNumber = "MC001", OwnerId = 1 },
            new() { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.2m, Color = "Cream", MicrochipNumber = "MC002", OwnerId = 1 },
            new() { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 35.0m, Color = "Black/Tan", MicrochipNumber = "MC003", OwnerId = 2 },
            new() { Id = 4, Name = "Luna", Species = "Cat", Breed = "Persian", DateOfBirth = new DateOnly(2022, 5, 8), Weight = 3.8m, Color = "White", OwnerId = 2 },
            new() { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2021, 11, 3), Weight = 12.0m, Color = "Tricolor", MicrochipNumber = "MC005", OwnerId = 3 },
            new() { Id = 6, Name = "Bella", Species = "Dog", Breed = "Labrador", DateOfBirth = new DateOnly(2020, 9, 18), Weight = 28.0m, Color = "Chocolate", OwnerId = 4 },
            new() { Id = 7, Name = "Oliver", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2021, 4, 25), Weight = 6.5m, Color = "Tabby", MicrochipNumber = "MC007", OwnerId = 4 },
            new() { Id = 8, Name = "Coco", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 1.8m, Color = "Brown", OwnerId = 5 },
        };
        context.Pets.AddRange(pets);

        // Veterinarians
        var vets = new List<Veterinarian>
        {
            new() { Id = 1, FirstName = "Dr. Amanda", LastName = "Chen", Email = "a.chen@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-001", HireDate = new DateOnly(2018, 6, 1) },
            new() { Id = 2, FirstName = "Dr. Robert", LastName = "Martinez", Email = "r.martinez@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-002", HireDate = new DateOnly(2019, 3, 15) },
            new() { Id = 3, FirstName = "Dr. Lisa", LastName = "Patel", Email = "l.patel@happypaws.com", Phone = "555-0203", Specialization = "Dermatology", LicenseNumber = "VET-003", HireDate = new DateOnly(2021, 1, 10) },
        };
        context.Veterinarians.AddRange(vets);

        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Appointments (various statuses)
        var appointments = new List<Appointment>
        {
            new() { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = now.AddDays(-10), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup" },
            new() { Id = 2, PetId = 2, VeterinarianId = 1, AppointmentDate = now.AddDays(-7), DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Dental cleaning" },
            new() { Id = 3, PetId = 3, VeterinarianId = 2, AppointmentDate = now.AddDays(-5), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Spay surgery" },
            new() { Id = 4, PetId = 5, VeterinarianId = 1, AppointmentDate = now.AddDays(-3), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Vaccination booster" },
            new() { Id = 5, PetId = 4, VeterinarianId = 3, AppointmentDate = now.AddDays(-1), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Skin rash examination" },
            new() { Id = 6, PetId = 6, VeterinarianId = 2, AppointmentDate = now.AddDays(-2), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Hip examination", CancellationReason = "Owner rescheduled" },
            new() { Id = 7, PetId = 1, VeterinarianId = 1, AppointmentDate = now.Date.AddHours(10), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up visit" },
            new() { Id = 8, PetId = 7, VeterinarianId = 3, AppointmentDate = now.Date.AddHours(14), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Skin allergy treatment" },
            new() { Id = 9, PetId = 3, VeterinarianId = 2, AppointmentDate = now.AddDays(3), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Post-surgery checkup" },
            new() { Id = 10, PetId = 8, VeterinarianId = 1, AppointmentDate = now.AddDays(5), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "General wellness exam" },
        };
        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync();

        // Medical Records (for completed appointments)
        var records = new List<MedicalRecord>
        {
            new() { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy, all vitals normal", Treatment = "No treatment required. Administered annual vaccines.", FollowUpDate = today.AddMonths(12) },
            new() { Id = 2, AppointmentId = 2, PetId = 2, VeterinarianId = 1, Diagnosis = "Mild tartar buildup on molars", Treatment = "Professional dental cleaning performed under light sedation.", Notes = "Recommend dental treats and regular brushing" },
            new() { Id = 3, AppointmentId = 3, PetId = 3, VeterinarianId = 2, Diagnosis = "Routine spay procedure", Treatment = "Ovariohysterectomy performed successfully. No complications.", FollowUpDate = today.AddDays(10), Notes = "Elizabethan collar required for 10 days" },
            new() { Id = 4, AppointmentId = 4, PetId = 5, VeterinarianId = 1, Diagnosis = "Healthy, due for boosters", Treatment = "Administered DHPP and Bordetella vaccines" },
        };
        context.MedicalRecords.AddRange(records);
        await context.SaveChangesAsync();

        // Prescriptions
        var prescriptions = new List<Prescription>
        {
            new() { Id = 1, MedicalRecordId = 2, MedicationName = "Clindamycin", Dosage = "25mg twice daily", DurationDays = 10, StartDate = today.AddDays(-7), Instructions = "Give with food" },
            new() { Id = 2, MedicalRecordId = 3, MedicationName = "Meloxicam", Dosage = "0.1mg/kg once daily", DurationDays = 5, StartDate = today.AddDays(-5), Instructions = "Pain management post-surgery" },
            new() { Id = 3, MedicalRecordId = 3, MedicationName = "Cephalexin", Dosage = "250mg twice daily", DurationDays = 14, StartDate = today.AddDays(-5), Instructions = "Complete full course of antibiotics" },
            new() { Id = 4, MedicalRecordId = 1, MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly", DurationDays = 30, StartDate = today, Instructions = "Monthly heartworm prevention" },
            new() { Id = 5, MedicalRecordId = 4, MedicationName = "Frontline Plus", Dosage = "1 application monthly", DurationDays = 30, StartDate = today.AddDays(-3), Instructions = "Apply between shoulder blades" },
        };
        context.Prescriptions.AddRange(prescriptions);

        // Vaccinations
        var vaccinations = new List<Vaccination>
        {
            new() { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = today.AddDays(-10), ExpirationDate = today.AddYears(1), BatchNumber = "RAB-2024-001", AdministeredByVetId = 1 },
            new() { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = today.AddDays(-10), ExpirationDate = today.AddYears(1), BatchNumber = "DHPP-2024-042", AdministeredByVetId = 1 },
            new() { Id = 3, PetId = 2, VaccineName = "FVRCP", DateAdministered = today.AddMonths(-11), ExpirationDate = today.AddDays(20), BatchNumber = "FVRCP-2024-015", AdministeredByVetId = 1, Notes = "Due for booster soon" },
            new() { Id = 4, PetId = 3, VaccineName = "Rabies", DateAdministered = today.AddMonths(-6), ExpirationDate = today.AddMonths(6), BatchNumber = "RAB-2024-033", AdministeredByVetId = 2 },
            new() { Id = 5, PetId = 5, VaccineName = "DHPP", DateAdministered = today.AddDays(-3), ExpirationDate = today.AddYears(1), BatchNumber = "DHPP-2024-089", AdministeredByVetId = 1 },
            new() { Id = 6, PetId = 5, VaccineName = "Bordetella", DateAdministered = today.AddDays(-3), ExpirationDate = today.AddMonths(6), BatchNumber = "BORD-2024-022", AdministeredByVetId = 1 },
        };
        context.Vaccinations.AddRange(vaccinations);

        await context.SaveChangesAsync();
    }
}
