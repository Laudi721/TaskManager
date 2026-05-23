using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Forge.Common.Interfaces;
using Forge.Common.Security;
using Forge.Database;
using Forge.Desk.WebApi.Configuration;
using Forge.Desk.WebApi.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ForgeDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

AddJwtAuthentication(builder);
AddScoped(builder);

var app = builder.Build();

CheckDatabaseCompatibility(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AngularDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Metoda do rejestracji serwisów o zakresie Scoped
void AddScoped(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<IPasswordService, PasswordService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<DatabaseSeedData>();

    builder.Services.Configure<MenuOptions>(builder.Configuration.GetSection(MenuOptions.SectionName));
    builder.Services.AddSingleton<IMenuProvider, XmlMenuProvider>();
}

void AddJwtAuthentication(WebApplicationBuilder builder)
{
    var jwt = builder.Configuration.GetSection("Jwt");
    var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

    builder.Services.AddAuthorization();
}

// Metoda do sprawdzania kompatybilności bazy danych, wykonywania migracji i seeding danych
void CheckDatabaseCompatibility(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ForgeDbContext>();
        var seeder = services.GetRequiredService<DatabaseSeedData>();

        context.Database.Migrate();

        seeder.Seed(context);
    }
}
