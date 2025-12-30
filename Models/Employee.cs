using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public class Employee
    {
        public long EmployeeId { get; set; }
        
        public long UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        public DateTime DOB { get; set; }
        
        [Required]
        public string Designation { get; set; } = string.Empty;
        
        [Required]
        public string Department { get; set; } = string.Empty;
        
        public string CostCenter { get; set; } = string.Empty;
        
        public DateTime DateOfJoining { get; set; }
        
        public string ContactNumber { get; set; } = string.Empty;
        
        public ICollection<Punch> Punches { get; set; } = new List<Punch>();
        public ICollection<Leave> Leaves { get; set; } = new List<Leave>();
    }
}