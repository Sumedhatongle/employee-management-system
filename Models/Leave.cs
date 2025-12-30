using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public class Leave
    {
        public long LeaveId { get; set; }
        
        public long EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        
        [Required]
        public LeaveType LeaveType { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
        
        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;
        
        public long? ReviewedBy { get; set; }
        
        public DateTime? ReviewedOn { get; set; }
    }

    public enum LeaveType
    {
        Casual,
        Sick,
        Other
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected
    }
}