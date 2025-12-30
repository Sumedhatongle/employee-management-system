using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly EmployeeDbContext _context;

        public RegisterModel(EmployeeDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public RegisterRequest RegisterRequest { get; set; } = new();

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
                if (await _context.Users.AnyAsync(u => u.Username == RegisterRequest.Username || u.Email == RegisterRequest.Email))
                {
                    ErrorMessage = "Username or email already exists";
                    return Page();
                }

                var user = new User
                {
                    Username = RegisterRequest.Username,
                    Email = RegisterRequest.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(RegisterRequest.Password),
                    Role = UserRole.Employee,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var employee = new Models.Employee
                {
                    UserId = user.Id,
                    FirstName = RegisterRequest.FirstName,
                    LastName = RegisterRequest.LastName,
                    Designation = RegisterRequest.Designation,
                    Department = RegisterRequest.Department,
                    CostCenter = RegisterRequest.CostCenter,
                    DateOfJoining = RegisterRequest.DateOfJoining,
                    ContactNumber = RegisterRequest.ContactNumber
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                SuccessMessage = "Registration successful! You can now login.";
                ModelState.Clear();
                RegisterRequest = new();
                
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration failed: {ex.Message}";
                return Page();
            }
        }
    }

    public class RegisterRequest
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