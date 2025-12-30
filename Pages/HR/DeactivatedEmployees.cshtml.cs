using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagement.Pages.HR
{
    [Authorize(Roles = "HR")]
    public class DeactivatedEmployeesModel : PageModel
    {
        private readonly EmployeeDbContext _context;

        public DeactivatedEmployeesModel(EmployeeDbContext context)
        {
            _context = context;
        }

        public List<EmployeeProfile> DeactivatedEmployees { get; set; } = new();

        public async Task OnGetAsync()
        {
            DeactivatedEmployees = await _context.Employees
                .Include(e => e.User)
                .Where(e => !e.User.IsActive)
                .Select(e => new EmployeeProfile
                {
                    EmployeeId = e.EmployeeId,
                    Username = e.User.Username,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Designation = e.Designation,
                    Department = e.Department,
                    ContactNumber = e.ContactNumber,
                    DateOfJoining = e.DateOfJoining
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostReactivateEmployeeAsync(long id)
        {
            try
            {
                var employee = await _context.Employees
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.EmployeeId == id);

                if (employee?.User != null)
                {
                    employee.User.IsActive = true;
                    await _context.SaveChangesAsync();
                }

                return RedirectToPage();
            }
            catch
            {
                return RedirectToPage();
            }
        }
    }
}