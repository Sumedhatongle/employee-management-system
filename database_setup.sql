-- Employee Management Database Views

-- 1. Employee Profile View
CREATE VIEW vw_EmployeeProfile AS
SELECT 
    e.EmployeeId,
    u.Username,
    e.FirstName,
    e.LastName,
    e.Designation,
    e.Department,
    e.ContactNumber,
    e.DateOfJoining
FROM Employees e
JOIN Users u ON e.UserId = u.Id
WHERE u.IsActive = 1;

-- 2. Punch Summary View (Daily aggregated punches)
CREATE VIEW vw_PunchSummary AS
SELECT 
    p.EmployeeId,
    DATE(p.Timestamp) as PunchDate,
    MIN(CASE WHEN p.PunchType = 'IN' THEN p.Timestamp END) as FirstIn,
    MAX(CASE WHEN p.PunchType = 'OUT' THEN p.Timestamp END) as LastOut
FROM Punches p
GROUP BY p.EmployeeId, DATE(p.Timestamp);

-- 3. Leave Requests View
CREATE VIEW vw_LeaveRequests AS
SELECT 
    l.LeaveId,
    l.EmployeeId,
    CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName,
    l.LeaveType,
    l.StartDate,
    l.EndDate,
    l.Reason,
    l.Status,
    l.AppliedOn
FROM Leaves l
JOIN Employees e ON l.EmployeeId = e.EmployeeId
JOIN Users u ON e.UserId = u.Id
WHERE u.IsActive = 1;

-- Sample data insertion (for testing)
-- Note: Run this after the application creates the tables

-- Insert sample employee (password: employee123)
INSERT INTO Users (Username, PasswordHash, Email, Role, IsActive) VALUES 
('john.doe', '$2a$11$rQZJKjMZjKjMZjKjMZjKjO7VjKjMZjKjMZjKjMZjKjMZjKjMZjKjM', 'john.doe@company.com', 'Employee', 1);

INSERT INTO Employees (UserId, FirstName, LastName, Designation, Department, CostCenter, DateOfJoining, ContactNumber) VALUES 
(LAST_INSERT_ID(), 'John', 'Doe', 'Software Developer', 'IT', 'IT001', '2024-01-15', '+1234567890');

-- Insert sample punches
INSERT INTO Punches (EmployeeId, PunchType, Timestamp, Location) VALUES 
(1, 'IN', '2024-12-22 09:00:00', 'Office'),
(1, 'OUT', '2024-12-22 18:00:00', 'Office');

-- Insert sample leave
INSERT INTO Leaves (EmployeeId, LeaveType, StartDate, EndDate, Reason, Status, AppliedOn) VALUES 
(1, 'Casual', '2024-12-25', '2024-12-26', 'Christmas holidays', 'Pending', NOW());