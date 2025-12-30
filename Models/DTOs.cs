using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public long EmployeeId { get; set; }
    }

    public class CreateEmployeeRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
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
        
        public DateTime DateOfJoining { get; set; } = DateTime.Today;
        
        public string ContactNumber { get; set; } = string.Empty;
    }

    public class PunchRequest
    {
        public long EmployeeId { get; set; }
        
        [Required]
        public string PunchType { get; set; } = string.Empty; // "IN" or "OUT"
        
        public DateTime? Timestamp { get; set; }
        
        public string? Location { get; set; }
    }
}