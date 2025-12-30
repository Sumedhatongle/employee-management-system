using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Pages.HR
{
    [Authorize(Roles = "HR")]
    public class EditEmployeeModel : PageModel
    {
        private readonly EmployeeDbContext _context;

        public EditEmployeeModel(EmployeeDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public EditEmployeeRequest EmployeeRequest { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            EmployeeRequest = new EditEmployeeRequest
            {
                EmployeeId = employee.EmployeeId,
                Username = employee.User.Username,
                Email = employee.User.Email,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Designation = employee.Designation,
                Department = employee.Department,
                CostCenter = employee.CostCenter ?? "",
                DateOfJoining = employee.DateOfJoining,
                ContactNumber = employee.ContactNumber,
                IsActive = employee.User.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var employee = await _context.Employees
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.EmployeeId == EmployeeRequest.EmployeeId);

                if (employee == null)
                {
                    ErrorMessage = "Employee not found";
                    return Page();
                }

                // Update user
                employee.User.Username = EmployeeRequest.Username;
                employee.User.Email = EmployeeRequest.Email;
                employee.User.IsActive = EmployeeRequest.IsActive;

                // Update employee
                employee.FirstName = EmployeeRequest.FirstName;
                employee.LastName = EmployeeRequest.LastName;
                employee.Designation = EmployeeRequest.Designation;
                employee.Department = EmployeeRequest.Department;
                employee.CostCenter = EmployeeRequest.CostCenter;
                employee.DateOfJoining = EmployeeRequest.DateOfJoining;
                employee.ContactNumber = EmployeeRequest.ContactNumber;

                await _context.SaveChangesAsync();

                SuccessMessage = "Employee updated successfully!";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating employee: {ex.Message}";
                return Page();
            }
        }
    }

    public class EditEmployeeRequest
    {
        public long EmployeeId { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Designation { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        public string CostCenter { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfJoining { get; set; }

        [Required]
        public string ContactNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}