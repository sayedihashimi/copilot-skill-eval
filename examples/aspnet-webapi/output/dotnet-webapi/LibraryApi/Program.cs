using System.Text.Json.Serialization;
using LibraryApi.Data;
using LibraryApi.Endpoints;
using LibraryApi.Middleware;
using LibraryApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IPatronService, PatronService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IFineService, FineService>();

// OpenAPI
builder.Services.AddOpenApi();

// JSON serialization: enums as strings
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Error handling
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    db.Database.Migrate();
    await DataSeeder.SeedAsync(db);
}

// Middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map endpoints
app.MapAuthorEndpoints();
app.MapCategoryEndpoints();
app.MapBookEndpoints();
app.MapPatronEndpoints();
app.MapLoanEndpoints();
app.MapReservationEndpoints();
app.MapFineEndpoints();

app.Run();
