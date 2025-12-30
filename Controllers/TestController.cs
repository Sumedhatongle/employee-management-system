using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagement.Services;
using EmployeeManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly EmployeeDbContext _context;

        public TestController(IJwtService jwtService, EmployeeDbContext context)
        {
            _jwtService = jwtService;
            _context = context;
        }

        [HttpGet("generate-token")]
        public async Task<IActionResult> GenerateToken(string username = "admin")
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return BadRequest(new { message = "User not found" });
                }

                var token = _jwtService.GenerateToken(user, user.Employee?.EmployeeId);

                return Ok(new
                {
                    message = "Token generated successfully",
                    token = token,
                    username = user.Username,
                    role = user.Role.ToString(),
                    employeeId = user.Employee?.EmployeeId,
                    expiresIn = "60 minutes",
                    usage = "Copy this token and use in Authorization header as: Bearer " + token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Token generation failed", error = ex.Message });
            }
        }

        [HttpGet("jwt")]
        public async Task<IActionResult> TestJwt()
        {
            try
            {
                // Get admin user
                var user = await _context.Users
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.Username == "admin");

                if (user == null)
                {
                    return BadRequest(new { message = "Admin user not found" });
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user, user.Employee?.EmployeeId);

                // Validate the token
                var principal = _jwtService.ValidateToken(token);

                return Ok(new
                {
                    message = "JWT is working correctly",
                    token = token,
                    isValid = principal != null,
                    claims = principal?.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "JWT test failed", error = ex.Message });
            }
        }

        [HttpGet("protected")]
        [Authorize]
        public IActionResult TestProtected()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(new
            {
                message = "Protected endpoint accessed successfully",
                user = User.Identity?.Name,
                role = User.FindFirst("role")?.Value,
                employeeId = User.FindFirst("EmployeeId")?.Value,
                claims = claims
            });
        }

        [HttpGet("debug-test")]
        public IActionResult DebugTest()
        {
            var message = "Debug test hit!"; // Add breakpoint here
            Console.WriteLine(message);
            return Ok(new { message, timestamp = DateTime.Now });
        }

        [HttpGet("hr-only")]
        [Authorize(Roles = "HR")]
        public IActionResult TestHROnly()
        {
            return Ok(new
            {
                message = "HR-only endpoint accessed successfully",
                user = User.Identity?.Name,
                role = User.FindFirst("role")?.Value
            });
        }
    }
}