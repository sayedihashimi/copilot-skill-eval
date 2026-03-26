using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.Endpoints;
using VetClinicApi.Middleware;
using VetClinicApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON serialization — enums as strings
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Configure EF Core with SQLite
builder.Services.AddDbContext<VetClinicDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IVeterinarianService, VeterinarianService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IVaccinationService, VaccinationService>();

// Configure OpenAPI
builder.Services.AddOpenApi();

// Configure global error handling
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Apply migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
    dbContext.Database.Migrate();
}

// Middleware pipeline
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map all API endpoints
app.MapOwnerEndpoints();
app.MapPetEndpoints();
app.MapVeterinarianEndpoints();
app.MapAppointmentEndpoints();
app.MapMedicalRecordEndpoints();
app.MapPrescriptionEndpoints();
app.MapVaccinationEndpoints();

app.Run();
