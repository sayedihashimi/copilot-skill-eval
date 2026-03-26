using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.Middleware;
using VetClinicApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<VetClinicDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IVeterinarianService, VeterinarianService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IVaccinationService, VaccinationService>();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// OpenAPI/Swagger
app.MapOpenApi();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "VetClinicApi v1");
        options.RoutePrefix = "swagger";
    });
}

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
    await context.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(context);
}

// --- API Endpoints ---

// Owners
var owners = app.MapGroup("/api/owners").WithTags("Owners");

owners.MapGet("/", async (IOwnerService service, string? search, int? page, int? pageSize) =>
{
    var pagination = new VetClinicApi.DTOs.PaginationParams { Page = page ?? 1, PageSize = pageSize ?? 10 };
    return Results.Ok(await service.GetAllAsync(search, pagination));
}).WithSummary("List all owners");

owners.MapGet("/{id:int}", async (IOwnerService service, int id) =>
{
    var owner = await service.GetByIdAsync(id);
    return owner is null ? Results.NotFound() : Results.Ok(owner);
}).WithSummary("Get owner by ID");

owners.MapPost("/", async (IOwnerService service, VetClinicApi.DTOs.CreateOwnerDto dto) =>
{
    var owner = await service.CreateAsync(dto);
    return Results.Created($"/api/owners/{owner.Id}", owner);
}).WithSummary("Create a new owner");

owners.MapPut("/{id:int}", async (IOwnerService service, int id, VetClinicApi.DTOs.UpdateOwnerDto dto) =>
{
    var owner = await service.UpdateAsync(id, dto);
    return owner is null ? Results.NotFound() : Results.Ok(owner);
}).WithSummary("Update an existing owner");

owners.MapDelete("/{id:int}", async (IOwnerService service, int id) =>
{
    var deleted = await service.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
}).WithSummary("Delete owner");

owners.MapGet("/{id:int}/pets", async (IOwnerService service, int id) =>
{
    return Results.Ok(await service.GetPetsAsync(id));
}).WithSummary("Get all pets for an owner");

owners.MapGet("/{id:int}/appointments", async (IOwnerService service, int id, int? page, int? pageSize) =>
{
    var pagination = new VetClinicApi.DTOs.PaginationParams { Page = page ?? 1, PageSize = pageSize ?? 10 };
    return Results.Ok(await service.GetAppointmentsAsync(id, pagination));
}).WithSummary("Get appointment history for an owner's pets");

// Pets
var pets = app.MapGroup("/api/pets").WithTags("Pets");

pets.MapGet("/", async (IPetService service, string? search, string? species, bool? includeInactive, int? page, int? pageSize) =>
{
    var pagination = new VetClinicApi.DTOs.PaginationParams { Page = page ?? 1, PageSize = pageSize ?? 10 };
    return Results.Ok(await service.GetAllAsync(search, species, includeInactive ?? false, pagination));
}).WithSummary("List all active pets");

pets.MapGet("/{id:int}", async (IPetService service, int id) =>
{
    var pet = await service.GetByIdAsync(id);
    return pet is null ? Results.NotFound() : Results.Ok(pet);
}).WithSummary("Get pet by ID");

pets.MapPost("/", async (IPetService service, VetClinicApi.DTOs.CreatePetDto dto) =>
{
    var pet = await service.CreateAsync(dto);
    return Results.Created($"/api/pets/{pet.Id}", pet);
}).WithSummary("Create a new pet");

pets.MapPut("/{id:int}", async (IPetService service, int id, VetClinicApi.DTOs.UpdatePetDto dto) =>
{
    var pet = await service.UpdateAsync(id, dto);
    return pet is null ? Results.NotFound() : Results.Ok(pet);
}).WithSummary("Update pet");

pets.MapDelete("/{id:int}", async (IPetService service, int id) =>
{
    var deleted = await service.SoftDeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
}).WithSummary("Soft-delete pet");

pets.MapGet("/{id:int}/medical-records", async (IPetService service, int id) =>
{
    return Results.Ok(await service.GetMedicalRecordsAsync(id));
}).WithSummary("Get all medical records for a pet");

pets.MapGet("/{id:int}/vaccinations", async (IPetService service, int id) =>
{
    return Results.Ok(await service.GetVaccinationsAsync(id));
}).WithSummary("Get all vaccinations for a pet");

pets.MapGet("/{id:int}/vaccinations/upcoming", async (IPetService service, int id) =>
{
    return Results.Ok(await service.GetUpcomingVaccinationsAsync(id));
}).WithSummary("Get vaccinations due soon or overdue");

pets.MapGet("/{id:int}/prescriptions/active", async (IPetService service, int id) =>
{
    return Results.Ok(await service.GetActivePrescriptionsAsync(id));
}).WithSummary("Get active prescriptions for a pet");

// Veterinarians
var vets = app.MapGroup("/api/veterinarians").WithTags("Veterinarians");

vets.MapGet("/", async (IVeterinarianService service, string? specialization, bool? isAvailable, int? page, int? pageSize) =>
{
    var pagination = new VetClinicApi.DTOs.PaginationParams { Page = page ?? 1, PageSize = pageSize ?? 10 };
    return Results.Ok(await service.GetAllAsync(specialization, isAvailable, pagination));
}).WithSummary("List all veterinarians");

vets.MapGet("/{id:int}", async (IVeterinarianService service, int id) =>
{
    var vet = await service.GetByIdAsync(id);
    return vet is null ? Results.NotFound() : Results.Ok(vet);
}).WithSummary("Get veterinarian details");

vets.MapPost("/", async (IVeterinarianService service, VetClinicApi.DTOs.CreateVeterinarianDto dto) =>
{
    var vet = await service.CreateAsync(dto);
    return Results.Created($"/api/veterinarians/{vet.Id}", vet);
}).WithSummary("Create a new veterinarian");

vets.MapPut("/{id:int}", async (IVeterinarianService service, int id, VetClinicApi.DTOs.UpdateVeterinarianDto dto) =>
{
    var vet = await service.UpdateAsync(id, dto);
    return vet is null ? Results.NotFound() : Results.Ok(vet);
}).WithSummary("Update veterinarian info");

vets.MapGet("/{id:int}/schedule", async (IVeterinarianService service, int id, DateOnly date) =>
{
    return Results.Ok(await service.GetScheduleAsync(id, date));
}).WithSummary("Get vet's appointments for a specific date");

vets.MapGet("/{id:int}/appointments", async (IVeterinarianService service, int id, string? status, int? page, int? pageSize) =>
{
    var pagination = new VetClinicApi.DTOs.PaginationParams { Page = page ?? 1, PageSize = pageSize ?? 10 };
    return Results.Ok(await service.GetAppointmentsAsync(id, status, pagination));
}).WithSummary("Get all appointments for a vet");

// Appointments
var appointments = app.MapGroup("/api/appointments").WithTags("Appointments");

appointments.MapGet("/", async (IAppointmentService service, DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, int? page, int? pageSize) =>
{
    var pagination = new VetClinicApi.DTOs.PaginationParams { Page = page ?? 1, PageSize = pageSize ?? 10 };
    return Results.Ok(await service.GetAllAsync(fromDate, toDate, status, vetId, petId, pagination));
}).WithSummary("List appointments");

appointments.MapGet("/{id:int}", async (IAppointmentService service, int id) =>
{
    var appt = await service.GetByIdAsync(id);
    return appt is null ? Results.NotFound() : Results.Ok(appt);
}).WithSummary("Get appointment details");

appointments.MapPost("/", async (IAppointmentService service, VetClinicApi.DTOs.CreateAppointmentDto dto) =>
{
    var appt = await service.CreateAsync(dto);
    return Results.Created($"/api/appointments/{appt.Id}", appt);
}).WithSummary("Schedule a new appointment");

appointments.MapPut("/{id:int}", async (IAppointmentService service, int id, VetClinicApi.DTOs.UpdateAppointmentDto dto) =>
{
    var appt = await service.UpdateAsync(id, dto);
    return appt is null ? Results.NotFound() : Results.Ok(appt);
}).WithSummary("Update appointment");

appointments.MapPatch("/{id:int}/status", async (IAppointmentService service, int id, VetClinicApi.DTOs.UpdateAppointmentStatusDto dto) =>
{
    var appt = await service.UpdateStatusAsync(id, dto);
    return appt is null ? Results.NotFound() : Results.Ok(appt);
}).WithSummary("Update appointment status");

appointments.MapGet("/today", async (IAppointmentService service) =>
{
    return Results.Ok(await service.GetTodayAsync());
}).WithSummary("Get today's appointments");

// Medical Records
var medicalRecords = app.MapGroup("/api/medical-records").WithTags("Medical Records");

medicalRecords.MapGet("/{id:int}", async (IMedicalRecordService service, int id) =>
{
    var record = await service.GetByIdAsync(id);
    return record is null ? Results.NotFound() : Results.Ok(record);
}).WithSummary("Get medical record with prescriptions");

medicalRecords.MapPost("/", async (IMedicalRecordService service, VetClinicApi.DTOs.CreateMedicalRecordDto dto) =>
{
    var record = await service.CreateAsync(dto);
    return Results.Created($"/api/medical-records/{record.Id}", record);
}).WithSummary("Create medical record");

medicalRecords.MapPut("/{id:int}", async (IMedicalRecordService service, int id, VetClinicApi.DTOs.UpdateMedicalRecordDto dto) =>
{
    var record = await service.UpdateAsync(id, dto);
    return record is null ? Results.NotFound() : Results.Ok(record);
}).WithSummary("Update medical record");

// Prescriptions
var prescriptions = app.MapGroup("/api/prescriptions").WithTags("Prescriptions");

prescriptions.MapGet("/{id:int}", async (IPrescriptionService service, int id) =>
{
    var rx = await service.GetByIdAsync(id);
    return rx is null ? Results.NotFound() : Results.Ok(rx);
}).WithSummary("Get prescription details");

prescriptions.MapPost("/", async (IPrescriptionService service, VetClinicApi.DTOs.CreatePrescriptionDto dto) =>
{
    var rx = await service.CreateAsync(dto);
    return Results.Created($"/api/prescriptions/{rx.Id}", rx);
}).WithSummary("Create prescription");

// Vaccinations
var vaccinations = app.MapGroup("/api/vaccinations").WithTags("Vaccinations");

vaccinations.MapPost("/", async (IVaccinationService service, VetClinicApi.DTOs.CreateVaccinationDto dto) =>
{
    var vax = await service.CreateAsync(dto);
    return Results.Created($"/api/vaccinations/{vax.Id}", vax);
}).WithSummary("Record a new vaccination");

vaccinations.MapGet("/{id:int}", async (IVaccinationService service, int id) =>
{
    var vax = await service.GetByIdAsync(id);
    return vax is null ? Results.NotFound() : Results.Ok(vax);
}).WithSummary("Get vaccination details");

app.Run();
