using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EmployeeManagement.Data;
using EmployeeManagement.Services;
using EmployeeManagement.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Database configuration
builder.Services.AddDbContext<EmployeeDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0))
    ).EnableSensitiveDataLogging()
     .LogTo(Console.WriteLine, LogLevel.Information));

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "JWT_OR_COOKIE";
    options.DefaultChallengeScheme = "JWT_OR_COOKIE";
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "EmployeeManagement",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "EmployeeManagement",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Check for JWT in cookie for web requests
            if (context.Request.Cookies.ContainsKey("AuthToken"))
            {
                context.Token = context.Request.Cookies["AuthToken"];
            }
            return Task.CompletedTask;
        }
    };
})
.AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        // Use JWT for API requests, Cookie for Razor Pages
        if (context.Request.Path.StartsWithSegments("/api"))
            return JwtBearerDefaults.AuthenticationScheme;
        return CookieAuthenticationDefaults.AuthenticationScheme;
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HROnly", policy => policy.RequireRole("HR"));
    options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
});

// Register services
builder.Services.AddScoped<IJwtService, JwtService>();

// CORS for mobile API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


// Add request logging middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"[REQUEST] {context.Request.Method} {context.Request.Path}");
    await next();
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Simple test endpoint
app.MapGet("/test-debug", () => {
    Console.WriteLine("[DEBUG] Test endpoint hit!");
    return "Debug endpoint working!";
});

// List all registered routes
app.MapGet("/debug/routes", (IServiceProvider serviceProvider) =>
{
    var endpointDataSource = serviceProvider.GetRequiredService<EndpointDataSource>();
    var endpoints = endpointDataSource.Endpoints
        .OfType<RouteEndpoint>()
        .Select(e => new { 
            Pattern = e.RoutePattern.RawText,
            Methods = string.Join(", ", e.Metadata.OfType<HttpMethodMetadata>().SelectMany(m => m.HttpMethods))
        })
        .ToList();
    return Results.Ok(endpoints);
});

// Create database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
    await context.Database.EnsureCreatedAsync();
    
    // Create database views
    await CreateDatabaseViews(context);
    
    // Seed HR user if not exists
    if (!await context.Users.AnyAsync(u => u.Role == UserRole.HR))
    {
        var hrUser = new User
        {
            Username = "admin",
            Email = "admin@company.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = UserRole.HR,
            IsActive = true
        };
        
        context.Users.Add(hrUser);
        await context.SaveChangesAsync();
    }
}

async Task CreateDatabaseViews(EmployeeDbContext context)
{
    try
    {
        // Drop views if they exist
        await context.Database.ExecuteSqlRawAsync("DROP VIEW IF EXISTS vw_EmployeeProfile");
        await context.Database.ExecuteSqlRawAsync("DROP VIEW IF EXISTS vw_PunchSummary");
        await context.Database.ExecuteSqlRawAsync("DROP VIEW IF EXISTS vw_LeaveRequests");
        
        // Create Employee Profile View
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE VIEW vw_EmployeeProfile AS
            SELECT 
                e.EmployeeId,
                u.Username,
                e.FirstName,
                e.LastName,
                e.Designation,
                e.Department,
                e.ContactNumber,
                e.DateOfJoining
            FROM Employees e
            JOIN Users u ON e.UserId = u.Id
            WHERE u.IsActive = 1
        ");
        
        // Create Punch Summary View
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE VIEW vw_PunchSummary AS
            SELECT 
                p.EmployeeId,
                DATE(p.Timestamp) as PunchDate,
                MIN(CASE WHEN p.PunchType = 'IN' THEN p.Timestamp END) as FirstIn,
                MAX(CASE WHEN p.PunchType = 'OUT' THEN p.Timestamp END) as LastOut
            FROM Punches p
            GROUP BY p.EmployeeId, DATE(p.Timestamp)
        ");
        
        // Create Leave Requests View
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE VIEW vw_LeaveRequests AS
            SELECT 
                l.LeaveId,
                l.EmployeeId,
                CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName,
                l.LeaveType,
                l.StartDate,
                l.EndDate,
                l.Reason,
                l.Status,
                l.AppliedOn
            FROM Leaves l
            JOIN Employees e ON l.EmployeeId = e.EmployeeId
            JOIN Users u ON e.UserId = u.Id
            WHERE u.IsActive = 1
        ");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating views: {ex.Message}");
    }
}

app.Run();