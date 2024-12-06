

# Web Service: Carbon Tracker
---

## **Table of Contents**
1. [Introduction](#introduction)
2. [Architecture Overview](#architecture-overview)
3. [RESTful API Design](#restful-api-design)
   - [API Endpoint Overview](#api-endpoint-overview)
4. [Authentication & Authorization](#authentication--authorization)
5. [Service Layer Pattern](#service-layer-pattern)
6. [File Storage Implementation](#file-storage-implementation)
7. [Best Practices](#best-practices)
8. [Configuration Management](#configuration-management)

---

## **1. Introduction**

In this document, we explore the design and implementation of a modern web service using ASP.NET Core, focusing on:
- RESTful API principles.
- JWT-based authentication and role-based authorization.
- Efficient service patterns and file storage techniques.
- Best practices for scalability, maintainability, and security.

---

## **2. Architecture Overview**

The web service follows a layered architecture to ensure clean separation of concerns:

1. **Controllers**:
   - Define API endpoints and handle HTTP requests/responses.
   - Delegate business logic to services.

2. **Services**:
   - Contain business logic for core functionalities (e.g., statistics, reports).

3. **Data Access Layer**:
   - Interacts with the database using Entity Framework Core.

4. **Models**:
   - Define the structure of data used throughout the application.

5. **Authentication & Authorization**:
   - Enforces secure access to resources.

---

## **3. RESTful API Design**

The API is built around RESTful principles, ensuring intuitive and predictable endpoint behavior.

### **Example: Emissions API Controller**
```csharp
[ApiController]
[Route("api/[controller]")]
public class EmissionsController : ControllerBase
{
    [HttpGet("{userId}")]
    public async Task<ActionResult<List<EmissionRecord>>> GetUserEmissions(string userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId != userId)
        {
            return Forbid();
        }

        var emissions = await _context.EmissionRecords
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
        
        return Ok(emissions);
    }

    [HttpPost]
    public async Task<ActionResult<EmissionRecord>> CreateEmission([FromBody] EmissionRecord emission)
    {
        _context.EmissionRecords.Add(emission);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(
            nameof(GetUserEmissions), 
            new { userId = emission.UserId }, 
            emission
        );
    }
}
```

---

### **API Endpoint Overview**

| **Endpoint**                | **Method** | **Purpose**                                              |
|------------------------------|------------|----------------------------------------------------------|
| `api/statistics/{userId}`    | GET        | Retrieve user-specific emissions statistics.             |
| `api/reports`                | GET        | Generate and download a CSV report for a date range.     |
| `api/emissions/{userId}`     | GET        | Fetch all emissions data for a specific user.            |
| `api/emissions`              | POST       | Add a new emission record.                               |
| `api/emissions/{id}`         | DELETE     | Delete an emission record by its ID.                     |

---

## **4. Authentication & Authorization**

The service uses **JWT-based authentication** for secure access and role-based authorization to enforce permissions.

### **Example: Login Endpoint**
```csharp
[AllowAnonymous]
[HttpPost("login")]
public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
{
    if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        return BadRequest("Invalid credentials");

    var token = await _authService.ValidateUserAsync(request.Email, request.Password);
    if (token == null)
        return Unauthorized("Invalid credentials");

    return Ok(new LoginResponse
    {
        Token = token,
        User = new UserData { /* user details */ }
    });
}
```

---

## **5. Service Layer Pattern**

The service layer encapsulates business logic for better separation of concerns.

### **Example: Report Service**
```csharp
public class ReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateEmissionsReportAsync(
        string userId, 
        DateTime startDate, 
        DateTime endDate)
    {
        var emissions = await _context.EmissionRecords
            .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
            .OrderBy(e => e.Date)
            .ToListAsync();

        var reportLines = new List<string>
        {
            "Date,Type,Source,Amount,Description"
        };

        foreach (var emission in emissions)
        {
            reportLines.Add($"{emission.Date:yyyy-MM-dd},{emission.Type},{emission.Source},{emission.Amount},{emission.Description}");
        }

        return System.Text.Encoding.UTF8.GetBytes(string.Join("\n", reportLines));
    }
}
```

---

## **6. File Storage Implementation**

CSV reports are dynamically generated in memory using the `ReportService`.

### **Example: Generating a Report**
```csharp
[HttpGet("api/reports")]
public async Task<IActionResult> GenerateReport([FromQuery] string userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
{
    var report = await _reportService.GenerateEmissionsReportAsync(userId, startDate, endDate);
    return File(report, "text/csv", $"Emissions_Report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv");
}
```

**How It Works**:
- The backend generates a CSV file in memory and returns it as a byte array.
- This avoids unnecessary disk I/O and ensures up-to-date reports.

---

## **7. Best Practices**

1. **Dependency Injection**:
   - Promotes loose coupling and testability.

2. **Async/Await Pattern**:
   - Improves scalability and resource utilization.

3. **Security**:
   - Implements JWT authentication and role-based authorization.

4. **Error Handling**:
   - Returns consistent error responses with proper HTTP status codes.

---

## **8. Configuration Management**

Application settings are stored in `appsettings.json` for maintainability and security.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "https://localhost:5001",
    "Audience": "https://localhost:5000"
  }
}
```


--
