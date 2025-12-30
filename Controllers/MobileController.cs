using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using EmployeeManagement.Services;
using BCrypt.Net;

namespace EmployeeManagement.Controllers
{
    [ApiController]
    [Route("api/mobile")]
    public class MobileController : ControllerBase
    {
        private readonly EmployeeDbContext _context;
        private readonly IJwtService _jwtService;

        public MobileController(EmployeeDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var token = _jwtService.GenerateToken(user, user.Employee?.EmployeeId);

            return Ok(new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.Role.ToString(),
                EmployeeId = user.Employee?.EmployeeId ?? 0
            });
        }

        [HttpPost("employees")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            try
            {
                // Check if username or email already exists
                if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
                {
                    return BadRequest(new { message = "Username or email already exists" });
                }

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = UserRole.Employee,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var employee = new Employee
                {
                    UserId = user.Id,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Designation = request.Designation,
                    Department = request.Department,
                    CostCenter = request.CostCenter,
                    DateOfJoining = request.DateOfJoining,
                    ContactNumber = request.ContactNumber
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Employee created successfully", employeeId = employee.EmployeeId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating employee", error = ex.Message });
            }
        }

        [HttpPost("punch")]
        [Authorize(Roles = "Employee,HR")]
        public async Task<IActionResult> PunchInOut([FromBody] PunchRequest request)
        {
            try
            {
                var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
                long employeeId;
                
                if (string.IsNullOrEmpty(employeeIdClaim) || !long.TryParse(employeeIdClaim, out employeeId))
                {
                    // For HR users without employee record, use a default employee ID for testing
                    if (User.IsInRole("HR"))
                    {
                        employeeId = 1; // Use first employee for HR testing
                    }
                    else
                    {
                        return BadRequest(new { message = "Invalid employee ID" });
                    }
                }

                if (!Enum.TryParse<PunchType>(request.PunchType.ToUpper(), out var punchType))
                {
                    return BadRequest(new { message = "Invalid punch type. Use 'IN' or 'OUT'" });
                }

                var punch = new Punch
                {
                    EmployeeId = employeeId,
                    PunchType = punchType,
                    Timestamp = request.Timestamp ?? DateTime.UtcNow,
                    Location = request.Location
                };

                _context.Punches.Add(punch);
                var result = await _context.SaveChangesAsync();
                
                // Log for verification
                Console.WriteLine($"Punch saved: ID={punch.PunchId}, Employee={employeeId}, Type={punchType}, Records affected={result}");

                return Ok(new { 
                    message = $"Punch {request.PunchType} recorded successfully", 
                    punchId = punch.PunchId,
                    timestamp = punch.Timestamp
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error recording punch", error = ex.Message });
            }
        }

        [HttpGet("verify-data")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> VerifyData()
        {
            try
            {
                var stats = new
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalEmployees = await _context.Employees.CountAsync(),
                    TotalPunches = await _context.Punches.CountAsync(),
                    TotalLeaves = await _context.Leaves.CountAsync(),
                    RecentPunches = await _context.Punches
                        .OrderByDescending(p => p.Timestamp)
                        .Take(5)
                        .Select(p => new { p.PunchId, p.EmployeeId, p.PunchType, p.Timestamp })
                        .ToListAsync()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error verifying data", error = ex.Message });
            }
        }

        [HttpGet("punches")]
        [Authorize(Roles = "Employee,HR")]
        public async Task<IActionResult> GetPunches([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                // Debug: Check user claims
                var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                var isHR = User.IsInRole("HR");
                var isEmployee = User.IsInRole("Employee");
                
                Console.WriteLine($"User Claims: {string.Join(", ", userClaims.Select(c => $"{c.Type}={c.Value}"))}");
                Console.WriteLine($"Is HR: {isHR}, Is Employee: {isEmployee}");
                
                var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
                long employeeId;
                
                Console.WriteLine($"EmployeeId Claim: {employeeIdClaim}");
                
                if (string.IsNullOrEmpty(employeeIdClaim) || !long.TryParse(employeeIdClaim, out employeeId))
                {
                    // For HR users without employee record, use a default employee ID for testing
                    if (User.IsInRole("HR"))
                    {
                        employeeId = 1; // Use first employee for HR testing
                        Console.WriteLine($"Using fallback Employee ID: {employeeId} for HR user");
                    }
                    else
                    {
                        Console.WriteLine("No valid employee ID found and user is not HR");
                        return BadRequest(new { message = "Invalid employee ID" });
                    }
                }

                var query = _context.Punches.Where(p => p.EmployeeId == employeeId);

                if (from.HasValue)
                    query = query.Where(p => p.Timestamp >= from.Value);

                if (to.HasValue)
                    query = query.Where(p => p.Timestamp <= to.Value);

                var punches = await query
                    .OrderByDescending(p => p.Timestamp)
                    .Select(p => new
                    {
                        p.PunchId,
                        p.PunchType,
                        p.Timestamp,
                        p.Location
                    })
                    .ToListAsync();

                return Ok(punches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching punches", error = ex.Message });
            }
        }
    }
}