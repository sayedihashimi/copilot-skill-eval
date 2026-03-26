using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(VetClinicDbContext db)
    {
        if (await db.Owners.AnyAsync())
            return;

        // --- Owners ---
        var owners = new List<Owner>
        {
            new()
            {
                FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com",
                Phone = "555-0101", Address = "123 Oak Street", City = "Springfield",
                State = "IL", ZipCode = "62701"
            },
            new()
            {
                FirstName = "Michael", LastName = "Chen", Email = "m.chen@email.com",
                Phone = "555-0102", Address = "456 Maple Avenue", City = "Portland",
                State = "OR", ZipCode = "97201"
            },
            new()
            {
                FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rod@email.com",
                Phone = "555-0103", Address = "789 Pine Road", City = "Austin",
                State = "TX", ZipCode = "78701"
            },
            new()
            {
                FirstName = "James", LastName = "Williams", Email = "j.williams@email.com",
                Phone = "555-0104", Address = "321 Elm Boulevard", City = "Denver",
                State = "CO", ZipCode = "80201"
            },
            new()
            {
                FirstName = "Lisa", LastName = "Thompson", Email = "lisa.t@email.com",
                Phone = "555-0105", Address = "654 Cedar Lane", City = "Seattle",
                State = "WA", ZipCode = "98101"
            }
        };
        db.Owners.AddRange(owners);
        await db.SaveChangesAsync();

        // --- Pets ---
        var pets = new List<Pet>
        {
            new()
            {
                Name = "Buddy", Species = "Dog", Breed = "Golden Retriever",
                DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m,
                Color = "Golden", MicrochipNumber = "MC-001-2020", OwnerId = owners[0].Id
            },
            new()
            {
                Name = "Whiskers", Species = "Cat", Breed = "Siamese",
                DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.8m,
                Color = "Cream", MicrochipNumber = "MC-002-2019", OwnerId = owners[0].Id
            },
            new()
            {
                Name = "Max", Species = "Dog", Breed = "German Shepherd",
                DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m,
                Color = "Black and Tan", MicrochipNumber = "MC-003-2021", OwnerId = owners[1].Id
            },
            new()
            {
                Name = "Luna", Species = "Cat", Breed = "Maine Coon",
                DateOfBirth = new DateOnly(2022, 5, 8), Weight = 6.2m,
                Color = "Tabby", MicrochipNumber = "MC-004-2022", OwnerId = owners[2].Id
            },
            new()
            {
                Name = "Rocky", Species = "Dog", Breed = "Bulldog",
                DateOfBirth = new DateOnly(2018, 11, 3), Weight = 25.0m,
                Color = "White and Brown", MicrochipNumber = "MC-005-2018", OwnerId = owners[2].Id
            },
            new()
            {
                Name = "Tweety", Species = "Bird", Breed = "Cockatiel",
                DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.09m,
                Color = "Yellow", OwnerId = owners[3].Id
            },
            new()
            {
                Name = "Bella", Species = "Dog", Breed = "Labrador Retriever",
                DateOfBirth = new DateOnly(2021, 9, 20), Weight = 28.5m,
                Color = "Chocolate", MicrochipNumber = "MC-007-2021", OwnerId = owners[3].Id
            },
            new()
            {
                Name = "Coco", Species = "Rabbit", Breed = "Holland Lop",
                DateOfBirth = new DateOnly(2023, 6, 1), Weight = 1.8m,
                Color = "Brown", OwnerId = owners[4].Id
            }
        };
        db.Pets.AddRange(pets);
        await db.SaveChangesAsync();

        // --- Veterinarians ---
        var vets = new List<Veterinarian>
        {
            new()
            {
                FirstName = "Amanda", LastName = "Foster", Email = "dr.foster@happypaws.com",
                Phone = "555-0201", Specialization = "General Practice",
                LicenseNumber = "VET-2015-001", HireDate = new DateOnly(2015, 6, 1)
            },
            new()
            {
                FirstName = "Robert", LastName = "Kim", Email = "dr.kim@happypaws.com",
                Phone = "555-0202", Specialization = "Surgery",
                LicenseNumber = "VET-2018-002", HireDate = new DateOnly(2018, 3, 15)
            },
            new()
            {
                FirstName = "Patricia", LastName = "Martinez", Email = "dr.martinez@happypaws.com",
                Phone = "555-0203", Specialization = "Dentistry",
                LicenseNumber = "VET-2020-003", HireDate = new DateOnly(2020, 1, 10)
            }
        };
        db.Veterinarians.AddRange(vets);
        await db.SaveChangesAsync();

        // --- Appointments ---
        var now = DateTime.UtcNow;
        var appointments = new List<Appointment>
        {
            // Completed appointments (in the past)
            new()
            {
                PetId = pets[0].Id, VeterinarianId = vets[0].Id,
                AppointmentDate = now.AddDays(-30), DurationMinutes = 30,
                Status = AppointmentStatus.Completed,
                Reason = "Annual wellness checkup", Notes = "All vitals normal"
            },
            new()
            {
                PetId = pets[1].Id, VeterinarianId = vets[0].Id,
                AppointmentDate = now.AddDays(-25), DurationMinutes = 45,
                Status = AppointmentStatus.Completed,
                Reason = "Dental cleaning", Notes = "Minor tartar buildup removed"
            },
            new()
            {
                PetId = pets[2].Id, VeterinarianId = vets[1].Id,
                AppointmentDate = now.AddDays(-20), DurationMinutes = 60,
                Status = AppointmentStatus.Completed,
                Reason = "Limping on right front leg", Notes = "X-ray taken, minor sprain"
            },
            new()
            {
                PetId = pets[4].Id, VeterinarianId = vets[2].Id,
                AppointmentDate = now.AddDays(-15), DurationMinutes = 30,
                Status = AppointmentStatus.Completed,
                Reason = "Skin irritation", Notes = "Prescribed medicated shampoo"
            },
            // Cancelled appointment
            new()
            {
                PetId = pets[3].Id, VeterinarianId = vets[0].Id,
                AppointmentDate = now.AddDays(-10), DurationMinutes = 30,
                Status = AppointmentStatus.Cancelled,
                Reason = "Vaccination booster",
                CancellationReason = "Owner had scheduling conflict"
            },
            // Scheduled future appointments
            new()
            {
                PetId = pets[0].Id, VeterinarianId = vets[0].Id,
                AppointmentDate = now.AddDays(3).Date.AddHours(9), DurationMinutes = 30,
                Status = AppointmentStatus.Scheduled,
                Reason = "Follow-up checkup"
            },
            new()
            {
                PetId = pets[3].Id, VeterinarianId = vets[0].Id,
                AppointmentDate = now.AddDays(3).Date.AddHours(10), DurationMinutes = 30,
                Status = AppointmentStatus.Scheduled,
                Reason = "Vaccination booster"
            },
            new()
            {
                PetId = pets[5].Id, VeterinarianId = vets[0].Id,
                AppointmentDate = now.AddDays(5).Date.AddHours(14), DurationMinutes = 30,
                Status = AppointmentStatus.Scheduled,
                Reason = "Wing feather trimming"
            },
            new()
            {
                PetId = pets[6].Id, VeterinarianId = vets[1].Id,
                AppointmentDate = now.AddDays(7).Date.AddHours(11), DurationMinutes = 45,
                Status = AppointmentStatus.Scheduled,
                Reason = "Spay surgery consultation"
            },
            new()
            {
                PetId = pets[7].Id, VeterinarianId = vets[2].Id,
                AppointmentDate = now.AddDays(10).Date.AddHours(15), DurationMinutes = 30,
                Status = AppointmentStatus.Scheduled,
                Reason = "Dental check and nail trimming"
            }
        };
        db.Appointments.AddRange(appointments);
        await db.SaveChangesAsync();

        // --- Medical Records (for completed appointments) ---
        var medicalRecords = new List<MedicalRecord>
        {
            new()
            {
                AppointmentId = appointments[0].Id, PetId = pets[0].Id, VeterinarianId = vets[0].Id,
                Diagnosis = "Healthy - no issues found during annual exam",
                Treatment = "Routine wellness examination completed. All vaccinations up to date.",
                Notes = "Weight is ideal. Teeth in good condition. Heart and lungs clear.",
                FollowUpDate = DateOnly.FromDateTime(now.AddMonths(12))
            },
            new()
            {
                AppointmentId = appointments[1].Id, PetId = pets[1].Id, VeterinarianId = vets[0].Id,
                Diagnosis = "Mild periodontal disease - Grade 1",
                Treatment = "Professional dental cleaning performed under light sedation. Tartar removed from upper molars.",
                Notes = "Recommend dental treats and regular tooth brushing at home.",
                FollowUpDate = DateOnly.FromDateTime(now.AddMonths(6))
            },
            new()
            {
                AppointmentId = appointments[2].Id, PetId = pets[2].Id, VeterinarianId = vets[1].Id,
                Diagnosis = "Mild soft tissue sprain in right front leg",
                Treatment = "Rest for 2 weeks, anti-inflammatory medication prescribed. No fracture seen on X-ray.",
                Notes = "Limit exercise to short leash walks. Recheck if limping persists after treatment."
            },
            new()
            {
                AppointmentId = appointments[3].Id, PetId = pets[4].Id, VeterinarianId = vets[2].Id,
                Diagnosis = "Contact dermatitis - likely environmental allergen",
                Treatment = "Medicated shampoo prescribed (chlorhexidine). Antihistamine for itching.",
                Notes = "Avoid known allergens. Consider allergy testing if symptoms recur.",
                FollowUpDate = DateOnly.FromDateTime(now.AddDays(14))
            }
        };
        db.MedicalRecords.AddRange(medicalRecords);
        await db.SaveChangesAsync();

        // --- Prescriptions ---
        var today = DateOnly.FromDateTime(now);
        var prescriptions = new List<Prescription>
        {
            new()
            {
                MedicalRecordId = medicalRecords[2].Id,
                MedicationName = "Carprofen (Rimadyl)",
                Dosage = "25mg twice daily",
                DurationDays = 14, StartDate = today.AddDays(-20),
                EndDate = today.AddDays(-6),
                Instructions = "Give with food. Monitor for vomiting or diarrhea."
            },
            new()
            {
                MedicalRecordId = medicalRecords[2].Id,
                MedicationName = "Glucosamine Supplement",
                Dosage = "500mg once daily",
                DurationDays = 60, StartDate = today.AddDays(-20),
                EndDate = today.AddDays(40),
                Instructions = "Mix with food. For joint support."
            },
            new()
            {
                MedicalRecordId = medicalRecords[3].Id,
                MedicationName = "Chlorhexidine Shampoo",
                Dosage = "Apply topically every 3 days",
                DurationDays = 21, StartDate = today.AddDays(-15),
                EndDate = today.AddDays(6),
                Instructions = "Lather and let sit for 10 minutes before rinsing."
            },
            new()
            {
                MedicalRecordId = medicalRecords[3].Id,
                MedicationName = "Diphenhydramine (Benadryl)",
                Dosage = "25mg twice daily",
                DurationDays = 10, StartDate = today.AddDays(-15),
                EndDate = today.AddDays(-5),
                Instructions = "Administer with food for itching relief."
            },
            new()
            {
                MedicalRecordId = medicalRecords[1].Id,
                MedicationName = "Clindamycin",
                Dosage = "75mg once daily",
                DurationDays = 7, StartDate = today.AddDays(-25),
                EndDate = today.AddDays(-18),
                Instructions = "Antibiotic for post-dental procedure. Complete full course."
            }
        };
        db.Prescriptions.AddRange(prescriptions);
        await db.SaveChangesAsync();

        // --- Vaccinations ---
        var vaccinations = new List<Vaccination>
        {
            // Current vaccinations
            new()
            {
                PetId = pets[0].Id, VaccineName = "Rabies",
                DateAdministered = today.AddMonths(-6), ExpirationDate = today.AddMonths(6),
                BatchNumber = "RAB-2024-A1", AdministeredByVetId = vets[0].Id,
                Notes = "3-year rabies vaccine"
            },
            new()
            {
                PetId = pets[0].Id, VaccineName = "DHPP",
                DateAdministered = today.AddMonths(-3), ExpirationDate = today.AddMonths(9),
                BatchNumber = "DHPP-2024-B2", AdministeredByVetId = vets[0].Id
            },
            // Expiring soon (within 30 days)
            new()
            {
                PetId = pets[2].Id, VaccineName = "Rabies",
                DateAdministered = today.AddMonths(-11), ExpirationDate = today.AddDays(20),
                BatchNumber = "RAB-2024-C3", AdministeredByVetId = vets[1].Id,
                Notes = "Due for renewal soon"
            },
            new()
            {
                PetId = pets[1].Id, VaccineName = "FVRCP",
                DateAdministered = today.AddMonths(-11), ExpirationDate = today.AddDays(15),
                BatchNumber = "FVR-2024-D4", AdministeredByVetId = vets[0].Id
            },
            // Already expired
            new()
            {
                PetId = pets[4].Id, VaccineName = "Bordetella",
                DateAdministered = today.AddMonths(-14), ExpirationDate = today.AddDays(-30),
                BatchNumber = "BOR-2023-E5", AdministeredByVetId = vets[2].Id,
                Notes = "Needs booster - overdue"
            },
            new()
            {
                PetId = pets[6].Id, VaccineName = "Rabies",
                DateAdministered = today.AddMonths(-2), ExpirationDate = today.AddMonths(10),
                BatchNumber = "RAB-2024-F6", AdministeredByVetId = vets[0].Id
            }
        };
        db.Vaccinations.AddRange(vaccinations);
        await db.SaveChangesAsync();
    }
}
