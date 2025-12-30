using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EmployeeManagement.Pages.Employee
{
    [Authorize(Roles = "Employee,HR")]
    public class DashboardModel : PageModel
    {
        private readonly EmployeeDbContext _context;

        public DashboardModel(EmployeeDbContext context)
        {
            _context = context;
        }

        public EmployeeProfile? CurrentEmployee { get; set; }
        public List<Punch> TodayPunches { get; set; } = new();
        public string LastPunchType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId > 0)
            {
                try
                {
                    CurrentEmployee = await _context.EmployeeProfiles
                        .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
                }
                catch
                {
                    // Fallback
                    var employee = await _context.Employees
                        .Include(e => e.User)
                        .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

                    if (employee != null)
                    {
                        CurrentEmployee = new EmployeeProfile
                        {
                            EmployeeId = employee.EmployeeId,
                            Username = employee.User.Username,
                            FirstName = employee.FirstName,
                            LastName = employee.LastName,
                            Designation = employee.Designation,
                            Department = employee.Department,
                            ContactNumber = employee.ContactNumber,
                            DateOfJoining = employee.DateOfJoining
                        };
                    }
                }

                var today = DateTime.Today;
                TodayPunches = await _context.Punches
                    .Where(p => p.EmployeeId == employeeId && p.Timestamp.Date == today)
                    .OrderByDescending(p => p.Timestamp)
                    .ToListAsync();

                LastPunchType = TodayPunches.FirstOrDefault()?.PunchType.ToString() ?? "";
            }
        }

        public async Task<IActionResult> OnPostPunchAsync(string punchType)
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId <= 0)
            {
                Message = "Employee not found";
                return Page();
            }

            if (!Enum.TryParse<PunchType>(punchType.ToUpper(), out var type))
            {
                Message = "Invalid punch type";
                return Page();
            }

            var punch = new Punch
            {
                EmployeeId = employeeId,
                PunchType = type,
                Timestamp = DateTime.UtcNow,
                Location = "Web Portal"
            };

            _context.Punches.Add(punch);
            await _context.SaveChangesAsync();

            Message = $"Punch {punchType} recorded successfully at {punch.Timestamp:HH:mm}";
            
            return RedirectToPage();
        }

        private long GetCurrentEmployeeId()
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (long.TryParse(employeeIdClaim, out var employeeId))
            {
                return employeeId;
            }
            return 1; // Fallback for demo
        }
    }
}