using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EmployeeManagement.Pages.Employee
{
    [Authorize(Roles = "Employee,HR")]
    public class PunchHistoryModel : PageModel
    {
        private readonly EmployeeDbContext _context;

        public PunchHistoryModel(EmployeeDbContext context)
        {
            _context = context;
        }

        public List<Punch> Punches { get; set; } = new();
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.Today;
        public string EmployeeName { get; set; } = string.Empty;

        public async Task OnGetAsync(DateTime? from = null, DateTime? to = null)
        {
            FromDate = from ?? DateTime.Today.AddDays(-30);
            ToDate = to ?? DateTime.Today;

            var employeeId = GetCurrentEmployeeId();
            if (employeeId > 0)
            {
                var employee = await _context.Employees
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
                
                if (employee != null)
                {
                    EmployeeName = $"{employee.FirstName} {employee.LastName}";
                }

                Punches = await _context.Punches
                    .Where(p => p.EmployeeId == employeeId && 
                               p.Timestamp.Date >= FromDate.Date && 
                               p.Timestamp.Date <= ToDate.Date)
                    .OrderByDescending(p => p.Timestamp)
                    .ToListAsync();
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