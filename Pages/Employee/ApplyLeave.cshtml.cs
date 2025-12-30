using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EmployeeManagement.Pages.Employee
{
    [Authorize(Roles = "Employee,HR")]
    public class ApplyLeaveModel : PageModel
    {
        private readonly EmployeeDbContext _context;

        public ApplyLeaveModel(EmployeeDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public LeaveApplicationRequest LeaveRequest { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public List<Models.Leave> RecentApplications { get; set; } = new();

        public async Task OnGetAsync()
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId > 0)
            {
                var employee = await _context.Employees
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
                
                if (employee != null)
                {
                    EmployeeName = $"{employee.FirstName} {employee.LastName}";
                    LeaveRequest.EmployeeId = employeeId;
                }

                RecentApplications = await _context.Leaves
                    .Where(l => l.EmployeeId == employeeId)
                    .OrderByDescending(l => l.AppliedOn)
                    .Take(5)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (LeaveRequest.StartDate < DateTime.Today)
            {
                ErrorMessage = "Start date cannot be in the past";
                return Page();
            }

            if (LeaveRequest.EndDate < LeaveRequest.StartDate)
            {
                ErrorMessage = "End date cannot be before start date";
                return Page();
            }

            try
            {
                var leave = new Models.Leave
                {
                    EmployeeId = LeaveRequest.EmployeeId,
                    LeaveType = LeaveRequest.LeaveType,
                    StartDate = LeaveRequest.StartDate,
                    EndDate = LeaveRequest.EndDate,
                    Reason = LeaveRequest.Reason,
                    Status = LeaveStatus.Pending,
                    AppliedOn = DateTime.UtcNow
                };

                _context.Leaves.Add(leave);
                await _context.SaveChangesAsync();

                var days = (LeaveRequest.EndDate - LeaveRequest.StartDate).Days + 1;
                SuccessMessage = $"Leave application submitted successfully! ({days} days from {LeaveRequest.StartDate:dd/MM/yyyy} to {LeaveRequest.EndDate:dd/MM/yyyy})";
                
                ModelState.Clear();
                LeaveRequest = new LeaveApplicationRequest { EmployeeId = LeaveRequest.EmployeeId };
                
                RecentApplications = await _context.Leaves
                    .Where(l => l.EmployeeId == LeaveRequest.EmployeeId)
                    .OrderByDescending(l => l.AppliedOn)
                    .Take(5)
                    .ToListAsync();
                
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error submitting leave application: {ex.Message}";
                return Page();
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

    public class LeaveApplicationRequest
    {
        public long EmployeeId { get; set; }
        
        [Required]
        public LeaveType LeaveType { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);
        
        [Required]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);
        
        [Required]
        [MinLength(10, ErrorMessage = "Please provide a detailed reason (minimum 10 characters)")]
        public string Reason { get; set; } = string.Empty;
    }
}