using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EmployeeManagement.Pages.Employee
{
    [Authorize(Roles = "Employee,HR")]
    public class ProfileModel : PageModel
    {
        private readonly EmployeeDbContext _context;

        public ProfileModel(EmployeeDbContext context)
        {
            _context = context;
        }

        public EmployeeProfile? CurrentEmployee { get; set; }
        public Models.User? CurrentUser { get; set; }

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

                        CurrentUser = employee.User;
                    }
                }

                if (CurrentUser == null && CurrentEmployee != null)
                {
                    CurrentUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == CurrentEmployee.Username);
                }
            }
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