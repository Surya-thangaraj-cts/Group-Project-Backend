using AccountTrack.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using UserApi.Repositories;
using UserApi.Services;
using UserApprovalApi.Data;
using UserApprovalApi.Models;
using UserApprovalApi.Repositories;
using UserApprovalApi.Services;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------

// CORS (Angular dev)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// EF Core - SQL Server
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// Services
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Controllers + JSON enum converter
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// JWT options/service
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<JwtService>();

// Authentication / Authorization
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyString = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(keyString))
{
    throw new InvalidOperationException(
        "JWT Key is not configured. Set a strong Jwt:Key in appsettings.json.");
}
var key = Encoding.UTF8.GetBytes(keyString!);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // set true in production with a proper cert
        options.SaveToken = true;
        options.MapInboundClaims = false;     // we use raw claim names ("role", "name")

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSection["Issuer"],         // "Company"
            ValidAudience = jwtSection["Audience"],     // "FrontendApp"
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = "role",
            NameClaimType = "name",
            ValidTypes = new[] { "JWT" } // ensure we only accept standard JWTs
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                Console.WriteLine("=== JWT MESSAGE RECEIVED ===");
                Console.WriteLine($"Authorization Header: {authHeader ?? "MISSING!"}");
                Console.WriteLine($"Token: {context.Token ?? "NULL"}");
                Console.WriteLine($"Path: {context.Request.Path}");
                Console.WriteLine("===========================");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("=== JWT AUTH FAILED ===");
                Console.WriteLine($"Exception: {context.Exception.Message}");
                Console.WriteLine($"Exception Type: {context.Exception.GetType().Name}");
                Console.WriteLine("======================");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("=== JWT CHALLENGE ===");
                Console.WriteLine($"Error: {context.Error ?? "none"}");
                Console.WriteLine($"ErrorDescription: {context.ErrorDescription ?? "none"}");
                Console.WriteLine($"AuthenticateFailure: {context.AuthenticateFailure?.Message ?? "none"}");
                Console.WriteLine("====================");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}");
                Console.WriteLine("=== JWT TOKEN VALIDATED ===");
                Console.WriteLine($"Claims: {string.Join(", ", claims ?? Array.Empty<string>())}");
                Console.WriteLine("===========================");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Swagger + JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UserApproval API",
        Version = "v1",
        Description = "User registration → admin approval → login with JWT"
    });

    // Proper bearer definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token. Example: Bearer {token}"
    });

    // Requirement
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// -------------------- Middleware --------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseHttpsRedirection();

app.UseAuthentication();   // must be before UseAuthorization
app.UseAuthorization();

app.MapControllers();

// -------------------- Database: migrate + seed --------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seed one Admin user if none exists
    if (!db.Users.Any(u => u.Role == UserRole.Admin))
    {
        var (hash, salt) = PasswordService.HashPassword("Admin@123!");
        db.Users.Add(new User
        {
            UserId = "admin",
            Name = "System Admin",
            Email = "admin@example.com",
            Branch = "HQ",
            Role = UserRole.Admin,
            PasswordHash = hash,
            PasswordSalt = salt,
            Status = UserStatus.Active
        });
        db.SaveChanges();
    }
}

app.Run();