using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagement.Pages.HR
{
    [Authorize(Roles = "HR")]
    public class ManageLeavesModel : PageModel
    {
        private readonly EmployeeDbContext _context;

        public ManageLeavesModel(EmployeeDbContext context)
        {
            _context = context;
        }

        public List<LeaveRequestView> LeaveRequests { get; set; } = new();
        public string StatusFilter { get; set; } = "Pending";

        public async Task OnGetAsync(string status = "Pending")
        {
            StatusFilter = status;

            try
            {
                var leaves = await _context.Leaves
                    .Include(l => l.Employee)
                    .ThenInclude(e => e.User)
                    .ToListAsync();

                var filteredLeaves = leaves.AsQueryable();

                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    if (Enum.TryParse<LeaveStatus>(status, out var statusEnum))
                    {
                        filteredLeaves = filteredLeaves.Where(l => l.Status == statusEnum);
                    }
                }

                LeaveRequests = filteredLeaves
                    .OrderByDescending(l => l.AppliedOn)
                    .Select(l => new LeaveRequestView
                    {
                        LeaveId = l.LeaveId,
                        EmployeeName = l.Employee.FirstName + " " + l.Employee.LastName,
                        LeaveType = l.LeaveType,
                        StartDate = l.StartDate,
                        EndDate = l.EndDate,
                        Reason = l.Reason,
                        Status = l.Status,
                        AppliedOn = l.AppliedOn,
                        Days = (l.EndDate - l.StartDate).Days + 1
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                // Fallback if there's an error
                LeaveRequests = new List<LeaveRequestView>();
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(long id)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave != null)
            {
                leave.Status = LeaveStatus.Approved;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { status = StatusFilter });
        }

        public async Task<IActionResult> OnPostRejectAsync(long id)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave != null)
            {
                leave.Status = LeaveStatus.Rejected;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { status = StatusFilter });
        }
    }

    public class LeaveRequestView
    {
        public long LeaveId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public LeaveType LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public LeaveStatus Status { get; set; }
        public DateTime AppliedOn { get; set; }
        public int Days { get; set; }
    }
}