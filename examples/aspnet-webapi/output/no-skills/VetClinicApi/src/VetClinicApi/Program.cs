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

// Controllers & OpenAPI
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

var app = builder.Build();

// Global exception handler
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// OpenAPI document endpoint
app.MapOpenApi();

// Serve Swagger UI via CDN
app.MapGet("/swagger", () => Results.Content("""
<!DOCTYPE html>
<html>
<head>
    <title>VetClinicApi - Swagger UI</title>
    <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css" />
</head>
<body>
    <div id="swagger-ui"></div>
    <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
    <script>
        SwaggerUIBundle({ url: '/openapi/v1.json', dom_id: '#swagger-ui' });
    </script>
</body>
</html>
""", "text/html")).ExcludeFromDescription();

app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(db);
}

app.Run();
