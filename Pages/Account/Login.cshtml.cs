using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeeManagement.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace EmployeeManagement.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly EmployeeDbContext _context;
        private readonly IJwtService _jwtService;

        public LoginModel(EmployeeDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [BindProperty]
        public LoginRequest LoginRequest { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var user = await _context.Users
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.Username == LoginRequest.Username && u.IsActive);

                if (user == null || !BCrypt.Net.BCrypt.Verify(LoginRequest.Password, user.PasswordHash))
                {
                    ErrorMessage = "Invalid username or password";
                    return Page();
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user, user.Employee?.EmployeeId);

                // Create claims for cookie authentication
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim("UserId", user.Id.ToString())
                };

                if (user.Employee != null)
                {
                    claims.Add(new Claim("EmployeeId", user.Employee.EmployeeId.ToString()));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                // Store JWT token in cookie for API calls
                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60)
                });

                // Redirect based on role
                if (user.Role == Models.UserRole.HR)
                {
                    return RedirectToPage("/HR/Dashboard");
                }
                else
                {
                    return RedirectToPage("/Employee/Dashboard");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
                return Page();
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}