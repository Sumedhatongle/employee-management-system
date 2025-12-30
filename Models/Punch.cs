using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public class Punch
    {
        public long PunchId { get; set; }
        
        public long EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        
        [Required]
        public PunchType PunchType { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public string? Location { get; set; }
    }

    public enum PunchType
    {
        IN,
        OUT
    }
}