using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagement.Pages.HR
{
    [Authorize(Roles = "HR")]
    public class DashboardModel : PageModel
    {
        private readonly EmployeeDbContext _context;

        public DashboardModel(EmployeeDbContext context)
        {
            _context = context;
        }

        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int TodaysPunches { get; set; }
        public List<EmployeeProfile> Employees { get; set; } = new();

        public async Task<IActionResult> OnPostDeactivateEmployeeAsync(long id)
        {
            try
            {
                var employee = await _context.Employees
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.EmployeeId == id);

                if (employee?.User != null)
                {
                    employee.User.IsActive = false;
                    await _context.SaveChangesAsync();
                }

                return RedirectToPage();
            }
            catch
            {
                return RedirectToPage();
            }
        }

        public async Task OnGetAsync()
        {
            Console.WriteLine("[FLOW] HR Dashboard OnGetAsync started");
            
            Console.WriteLine("[FLOW] Counting total employees...");
            TotalEmployees = await _context.Employees.CountAsync();
            Console.WriteLine($"[FLOW] Total employees: {TotalEmployees}");
            
            Console.WriteLine("[FLOW] Counting active employees...");
            ActiveEmployees = await _context.Users.CountAsync(u => u.IsActive);
            Console.WriteLine($"[FLOW] Active employees: {ActiveEmployees}");
            
            Console.WriteLine("[FLOW] Counting today's punches...");
            TodaysPunches = await _context.Punches.CountAsync(p => p.Timestamp.Date == DateTime.Today);
            Console.WriteLine($"[FLOW] Today's punches: {TodaysPunches}");

            try
            {
                Console.WriteLine("[FLOW] Loading employees from view...");
                Employees = await _context.EmployeeProfiles.ToListAsync();
                Console.WriteLine($"[FLOW] Loaded {Employees.Count} employees from view");
            }
            catch
            {
                Console.WriteLine("[FLOW] View failed, using fallback query...");
                // Fallback if view doesn't exist
                Employees = await _context.Employees
                    .Include(e => e.User)
                    .Where(e => e.User.IsActive)
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
                Console.WriteLine($"[FLOW] Loaded {Employees.Count} employees from fallback query");
            }
            
            Console.WriteLine("[FLOW] HR Dashboard OnGetAsync completed");
        }
    }
}