# Employee Management POC

A comprehensive Employee Management system built with ASP.NET Core 6 Razor Pages, 
MySQL, and JWT authentication.

## Features

- **HR Functions**: Create, edit, deactivate employee accounts
- **Employee Functions**: Login, punch In/Out, view punch history, apply for leave
- **Mobile API**: REST endpoints for employee creation and punch operations
- **JWT Authentication**: Secure token-based authentication
- **Database Views**: Optimized queries for reporting

## Technology Stack

- **Backend**: ASP.NET Core 9 (Razor Pages)
- **Authentication**: JWT (System.IdentityModel.Tokens.Jwt)
- **Database**: MySQL with Entity Framework Core
- **Password Hashing**: BCrypt.Net-Next
- **API**: RESTful endpoints for mobile integration

## Quick Start

### Prerequisites
- .NET 9.0 SDK
- MySQL Server
- Visual Studio

### Setup Instructions

1. **Clone and Navigate**
   ```bash
   cd EmployeeManagement
   ```

2. **Update Database Connection**
   - Edit `appsettings.json`
   - Update MySQL connection string with your credentials

3. **Run the Application**
   ```bash
   dotnet run
   ```

4. **Create Database Views** (Optional)
   - Run the SQL script in `database_setup.sql` in MySQL Workbench

## Default Credentials

- **HR Admin**: 
  - Username: `admin`
  - Password: `admin123`

## API Endpoints

### Authentication
- `POST /api/mobile/auth/login` - User login

### Employee Management (HR only)
- `POST /api/mobile/employees` - Create employee

### Punch Operations (Employee only)
- `POST /api/mobile/punch` - Punch In/Out
- `GET /api/mobile/punches` - Get punch history

## Sample API Requests

### Login
```json
POST /api/mobile/auth/login
{
  "username": "admin",
  "password": "admin123"
}
```

### Create Employee (HR token required)
```json
POST /api/mobile/employees
Authorization: Bearer <jwt_token>
{
  "username": "john.doe",
  "email": "john@company.com",
  "password": "employee123",
  "firstName": "John",
  "lastName": "Doe",
  "designation": "Developer",
  "department": "IT"
}
```

### Punch In/Out (Employee token required)
```json
POST /api/mobile/punch
Authorization: Bearer <jwt_token>
{
  "employeeId": 1,
  "punchType": "IN",
  "location": "Office"
}
```

## Database Schema

### Tables
- **Users**: Authentication and user roles
- **Employees**: Employee personal and job information
- **Punches**: Time tracking records
- **Leaves**: Leave applications and approvals

### Views
- **vw_EmployeeProfile**: Employee details with user info
- **vw_PunchSummary**: Daily punch aggregations
- **vw_LeaveRequests**: Leave requests with employee names

## Security Features

- JWT token-based authentication
- Role-based authorization (HR vs Employee)
- BCrypt password hashing
- HTTPS enforcement
- Input validation and sanitization

## Development Notes

- The application automatically creates the database on first run
- Default HR admin user is seeded automatically
- JWT tokens expire after 1 hour
- All API endpoints return JSON responses
- CORS is enabled for mobile API access

## Testing

Use the provided Postman collection or any HTTP client to test the API endpoints. Make sure to:

1. Login to get JWT token
2. Include `Authorization: Bearer <token>` header for protected endpoints
3. Use appropriate role-based tokens (HR for employee creation, Employee for punch operations)

## Production Considerations

- Update JWT secret key in production
- Configure proper HTTPS certificates
- Set up proper logging and monitoring
- Implement refresh tokens for extended sessions
- Add rate limiting and API throttling
- Configure proper CORS policies