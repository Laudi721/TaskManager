using Microsoft.EntityFrameworkCore;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Security;
using TaskManager.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TaskManagerDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

AddScoped(builder);

var app = builder.Build();

CheckDatabaseCompatibility(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void AddScoped(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<IPasswordService, PasswordService>();
    builder.Services.AddScoped<DatabaseSeedData>();
}

void CheckDatabaseCompatibility(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<TaskManagerDbContext>();
        var seeder = services.GetRequiredService<DatabaseSeedData>();

        // migracje (tworzy DB jeśli nie istnieje)
        context.Database.Migrate();

        // seed danych
        seeder.Seed(context);
    }
}