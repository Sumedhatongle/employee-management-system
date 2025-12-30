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
    public class CreateEmployeeModel : PageModel
    {
        private readonly EmployeeDbContext _context;

        public CreateEmployeeModel(EmployeeDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CreateEmployeeRequest EmployeeRequest { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                if (await _context.Users.AnyAsync(u => u.Username == EmployeeRequest.Username || u.Email == EmployeeRequest.Email))
                {
                    ErrorMessage = "Username or email already exists";
                    return Page();
                }

                var user = new User
                {
                    Username = EmployeeRequest.Username,
                    Email = EmployeeRequest.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(EmployeeRequest.Password),
                    Role = UserRole.Employee,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var employee = new Models.Employee
                {
                    UserId = user.Id,
                    FirstName = EmployeeRequest.FirstName,
                    LastName = EmployeeRequest.LastName,
                    Designation = EmployeeRequest.Designation,
                    Department = EmployeeRequest.Department,
                    CostCenter = EmployeeRequest.CostCenter,
                    DateOfJoining = EmployeeRequest.DateOfJoining,
                    ContactNumber = EmployeeRequest.ContactNumber
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                SuccessMessage = $"Employee {EmployeeRequest.FirstName} {EmployeeRequest.LastName} created successfully!";
                ModelState.Clear();
                EmployeeRequest = new();
                
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating employee: {ex.Message}";
                return Page();
            }
        }
    }

    public class CreateEmployeeRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

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
        public DateTime DateOfJoining { get; set; } = DateTime.Today;

        [Required]
        public string ContactNumber { get; set; } = string.Empty;
    }
}