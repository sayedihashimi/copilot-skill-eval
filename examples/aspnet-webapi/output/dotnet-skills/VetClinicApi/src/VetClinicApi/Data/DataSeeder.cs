using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(VetClinicDbContext context)
    {
        if (await context.Owners.AnyAsync()) return;

        // 5 Owners
        var owners = new List<Owner>
        {
            new() { FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701" },
            new() { FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Avenue", City = "Portland", State = "OR", ZipCode = "97201" },
            new() { FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301" },
            new() { FirstName = "David", LastName = "Thompson", Email = "david.thompson@email.com", Phone = "555-0104", Address = "321 Elm Drive", City = "Denver", State = "CO", ZipCode = "80201" },
            new() { FirstName = "Jessica", LastName = "Patel", Email = "jessica.patel@email.com", Phone = "555-0105", Address = "654 Birch Lane", City = "Seattle", State = "WA", ZipCode = "98101" }
        };
        context.Owners.AddRange(owners);
        await context.SaveChangesAsync();

        // 8 Pets
        var pets = new List<Pet>
        {
            new() { Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", OwnerId = owners[0].Id },
            new() { Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.5m, Color = "Cream", MicrochipNumber = "MC-002-2019", OwnerId = owners[0].Id },
            new() { Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", OwnerId = owners[1].Id },
            new() { Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 8), Weight = 6.2m, Color = "Tabby", OwnerId = owners[2].Id },
            new() { Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2021, 11, 3), Weight = 12.0m, Color = "Tricolor", MicrochipNumber = "MC-005-2021", OwnerId = owners[2].Id },
            new() { Name = "Coco", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 1.8m, Color = "Brown", OwnerId = owners[3].Id },
            new() { Name = "Rocky", Species = "Dog", Breed = "Rottweiler", DateOfBirth = new DateOnly(2020, 9, 1), Weight = 45.0m, Color = "Black and Mahogany", MicrochipNumber = "MC-007-2020", OwnerId = owners[3].Id },
            new() { Name = "Bella", Species = "Cat", Breed = "Persian", DateOfBirth = new DateOnly(2021, 6, 20), Weight = 4.0m, Color = "White", MicrochipNumber = "MC-008-2021", OwnerId = owners[4].Id }
        };
        context.Pets.AddRange(pets);
        await context.SaveChangesAsync();

        // 3 Veterinarians
        var vets = new List<Veterinarian>
        {
            new() { FirstName = "Dr. Amanda", LastName = "Wilson", Email = "amanda.wilson@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", HireDate = new DateOnly(2015, 6, 1) },
            new() { FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", HireDate = new DateOnly(2018, 3, 15) },
            new() { FirstName = "Dr. Lisa", LastName = "Martinez", Email = "lisa.martinez@happypaws.com", Phone = "555-0203", Specialization = "Dentistry", LicenseNumber = "VET-2020-003", HireDate = new DateOnly(2020, 1, 10) }
        };
        context.Veterinarians.AddRange(vets);
        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        // 10 Appointments in various statuses
        var appointments = new List<Appointment>
        {
            // Completed appointments (past)
            new() { PetId = pets[0].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-30), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup and vaccination" },
            new() { PetId = pets[1].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-25), DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Dental cleaning" },
            new() { PetId = pets[2].Id, VeterinarianId = vets[1].Id, AppointmentDate = now.AddDays(-20), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Knee surgery follow-up" },
            new() { PetId = pets[4].Id, VeterinarianId = vets[2].Id, AppointmentDate = now.AddDays(-15), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Tooth extraction" },
            // Cancelled
            new() { PetId = pets[3].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-10), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Routine checkup", CancellationReason = "Owner had a scheduling conflict" },
            // NoShow
            new() { PetId = pets[5].Id, VeterinarianId = vets[2].Id, AppointmentDate = now.AddDays(-5), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Nail trimming" },
            // Today - in progress
            new() { PetId = pets[6].Id, VeterinarianId = vets[1].Id, AppointmentDate = now.Date.AddHours(9), DurationMinutes = 45, Status = AppointmentStatus.InProgress, Reason = "Limping examination" },
            // Today - checked in
            new() { PetId = pets[7].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.Date.AddHours(11), DurationMinutes = 30, Status = AppointmentStatus.CheckedIn, Reason = "Skin allergy consultation" },
            // Future
            new() { PetId = pets[0].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(7).Date.AddHours(10), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up vaccination" },
            new() { PetId = pets[2].Id, VeterinarianId = vets[1].Id, AppointmentDate = now.AddDays(14).Date.AddHours(14), DurationMinutes = 60, Status = AppointmentStatus.Scheduled, Reason = "Post-surgery checkup" }
        };
        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync();

        // 4 Medical records (for completed appointments)
        var records = new List<MedicalRecord>
        {
            new() { AppointmentId = appointments[0].Id, PetId = pets[0].Id, VeterinarianId = vets[0].Id, Diagnosis = "Healthy. All vitals normal.", Treatment = "Annual vaccines administered. Heartworm test negative.", Notes = "Weight is ideal. Continue current diet.", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(6)) },
            new() { AppointmentId = appointments[1].Id, PetId = pets[1].Id, VeterinarianId = vets[0].Id, Diagnosis = "Mild gingivitis detected.", Treatment = "Professional dental cleaning performed. Scaling and polishing completed.", Notes = "Recommend dental treats and regular brushing." },
            new() { AppointmentId = appointments[2].Id, PetId = pets[2].Id, VeterinarianId = vets[1].Id, Diagnosis = "Post-surgical knee healing well.", Treatment = "Physical therapy exercises prescribed. Pain medication adjusted.", Notes = "Reduce activity for 2 more weeks.", FollowUpDate = DateOnly.FromDateTime(now.AddDays(14)) },
            new() { AppointmentId = appointments[3].Id, PetId = pets[4].Id, VeterinarianId = vets[2].Id, Diagnosis = "Fractured premolar requiring extraction.", Treatment = "Tooth extracted under anesthesia. Antibiotics prescribed.", Notes = "Soft food only for 7 days." }
        };
        context.MedicalRecords.AddRange(records);
        await context.SaveChangesAsync();

        // 5 Prescriptions
        var prescriptions = new List<Prescription>
        {
            new() { MedicalRecordId = records[0].Id, MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly", DurationDays = 180, StartDate = DateOnly.FromDateTime(now.AddDays(-30)), EndDate = DateOnly.FromDateTime(now.AddDays(150)), Instructions = "Give on the same day each month with food" },
            new() { MedicalRecordId = records[2].Id, MedicationName = "Carprofen", Dosage = "75mg twice daily", DurationDays = 14, StartDate = DateOnly.FromDateTime(now.AddDays(-20)), EndDate = DateOnly.FromDateTime(now.AddDays(-6)), Instructions = "Give with food. Monitor for GI upset." },
            new() { MedicalRecordId = records[2].Id, MedicationName = "Gabapentin", Dosage = "100mg three times daily", DurationDays = 21, StartDate = DateOnly.FromDateTime(now.AddDays(-20)), EndDate = DateOnly.FromDateTime(now.AddDays(1)), Instructions = "May cause drowsiness. Give with or without food." },
            new() { MedicalRecordId = records[3].Id, MedicationName = "Amoxicillin", Dosage = "250mg twice daily", DurationDays = 10, StartDate = DateOnly.FromDateTime(now.AddDays(-15)), EndDate = DateOnly.FromDateTime(now.AddDays(-5)), Instructions = "Complete full course. Give with food." },
            new() { MedicalRecordId = records[3].Id, MedicationName = "Meloxicam", Dosage = "0.1mg/kg once daily", DurationDays = 5, StartDate = DateOnly.FromDateTime(now.AddDays(-15)), EndDate = DateOnly.FromDateTime(now.AddDays(-10)), Instructions = "Give with food. Do not exceed prescribed dose." }
        };
        context.Prescriptions.AddRange(prescriptions);
        await context.SaveChangesAsync();

        // 6 Vaccinations
        var vaccinations = new List<Vaccination>
        {
            // Current
            new() { PetId = pets[0].Id, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddDays(-30)), ExpirationDate = DateOnly.FromDateTime(now.AddYears(1)), BatchNumber = "RAB-2024-001", AdministeredByVetId = vets[0].Id, Notes = "3-year rabies vaccine" },
            new() { PetId = pets[0].Id, VaccineName = "DHPP", DateAdministered = DateOnly.FromDateTime(now.AddDays(-30)), ExpirationDate = DateOnly.FromDateTime(now.AddYears(1)), BatchNumber = "DHPP-2024-015", AdministeredByVetId = vets[0].Id },
            // Expiring soon (within 30 days)
            new() { PetId = pets[2].Id, VaccineName = "Bordetella", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-11)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(20)), BatchNumber = "BOR-2024-008", AdministeredByVetId = vets[0].Id },
            new() { PetId = pets[4].Id, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-11)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(15)), BatchNumber = "RAB-2024-003", AdministeredByVetId = vets[2].Id },
            // Expired
            new() { PetId = pets[1].Id, VaccineName = "FVRCP", DateAdministered = DateOnly.FromDateTime(now.AddYears(-2)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(-60)), BatchNumber = "FVRCP-2023-012", AdministeredByVetId = vets[0].Id, Notes = "Due for booster" },
            new() { PetId = pets[7].Id, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddYears(-1).AddMonths(-3)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(-90)), BatchNumber = "RAB-2023-020", AdministeredByVetId = vets[2].Id }
        };
        context.Vaccinations.AddRange(vaccinations);
        await context.SaveChangesAsync();
    }
}
