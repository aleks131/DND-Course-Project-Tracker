

# **Web Application**

This document provides an overview of how the **Carbon Tracker** web application implements its core functionalities, how the frontend interacts with the backend, and the structure of its key pages.

---

## **Table of Contents**
1. [Key Requirements](#key-requirements)
   - [Measure CO₂ Emissions](#1-measure-co₂-emissions)
   - [Generate Emissions Reports](#2-generate-emissions-reports)
2. [Overview of Web Application Pages](#overview-of-web-application-pages)
3. [Frontend-to-Backend Communication](#frontend-to-backend-communication)
   - [Fetching Statistics](#fetching-statistics)
   - [Downloading Reports](#downloading-reports)


---

## **1. Key Requirements**

The primary functionality of the application is to **measure CO₂ emissions** for users, providing them with detailed statistics and downloadable reports. This is achieved through backend services and user-facing pages designed to interact with them seamlessly.

### **1. Measure CO₂ Emissions**
The application enables users to track their carbon footprint by calculating total emissions, daily and monthly averages, and categorized breakdowns.

#### **Backend Implementation**
The `StatisticsService` handles the core logic for emissions statistics:
- **Total Emissions**: The sum of all emissions recorded by a user.
- **Averages**: Monthly and daily averages calculated from emission records.
- **Categorized Data**: Emissions grouped by type (e.g., Transportation, Energy) and source.

**Example: Calculating Monthly Average**
```csharp
private double CalculateMonthlyAverage(List<EmissionRecord> emissions)
{
    if (!emissions.Any())
        return 0;

    var months = (emissions.Max(e => e.Date) - emissions.Min(e => e.Date)).Days / 30.0;
    return months > 0 ? (double)emissions.Sum(e => e.Amount) / months : 0;
}
```

**How It Works**:
- The method calculates the monthly average emissions over a given date range.
- It ensures accurate calculations even with missing data.

#### **Frontend Interaction**
The frontend retrieves user statistics from the backend and displays them on the dashboard:
```csharp
@inject HttpClient Http

protected override async Task OnInitializedAsync()
{
    var userId = "example-user-id";
    var statistics = await Http.GetFromJsonAsync<UserEmissionStatistics>($"api/statistics/{userId}");
    // Populate dashboard with statistics
}
```

**How It Works**:
- The frontend sends an HTTP GET request to the backend API to fetch statistics for the authenticated user.
- Data is dynamically displayed using Blazor components.

---

### **2. Generate Emissions Reports**
Users can generate and download custom emissions reports in CSV format for a specified date range.

#### **Backend Implementation**
The `ReportService` generates reports by fetching emissions data and formatting it as a CSV file.

**Example: Generating a Report**
```csharp
public async Task<byte[]> GenerateEmissionsReportAsync(string userId, DateTime startDate, DateTime endDate)
{
    var emissions = await _context.EmissionRecords
        .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
        .OrderBy(e => e.Date)
        .ToListAsync();

    var reportLines = new List<string> { "Date,Type,Source,Amount,Description" };
    foreach (var emission in emissions)
    {
        reportLines.Add($"{emission.Date:yyyy-MM-dd},{emission.Type},{emission.Source},{emission.Amount},{emission.Description}");
    }
    return System.Text.Encoding.UTF8.GetBytes(string.Join("\n", reportLines));
}
```

**How It Works**:
- Filters emissions by user and date range.
- Generates a CSV file with details of emissions for the specified period.

#### **Frontend Interaction**
The frontend triggers the report generation through a button on the Reports page:
```csharp
@inject HttpClient Http

private async Task DownloadReport()
{
    var startDate = new DateTime(2023, 1, 1);
    var endDate = DateTime.UtcNow;

    var reportBytes = await Http.GetByteArrayAsync($"api/reports?userId=example-user-id&startDate={startDate}&endDate={endDate}");
    var fileName = $"Emissions_Report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";
    await FileUtil.SaveAs(fileName, reportBytes);
}
```

**How It Works**:
- Sends an HTTP GET request to the backend to generate the report.
- The backend returns the report as a byte array, which the frontend converts into a downloadable CSV file.

---

## **2. Overview of Web Application Pages**

The application consists of the following key pages:

| **Page**                | **URL**         | **Purpose**                                                                                       |
|--------------------------|-----------------|---------------------------------------------------------------------------------------------------|
| **Homepage**             | `/`             | Introduces the platform, highlights features, and encourages user registration/login.             |
| **Dashboard**            | `/dashboard`    | Displays user-specific statistics, trends, recent activities, and eco-tips.                      |
| **Login Page**           | `/login`        | Allows users to securely log into their accounts.                                                 |
| **Registration Page**    | `/register`     | Enables new users to create an account to start tracking their carbon footprint.                 |

---

## **3. Frontend-to-Backend Communication**

The frontend communicates with the backend using `HttpClient` to retrieve data and execute actions. The following examples illustrate this integration:

### **Fetching Statistics**
The dashboard fetches user-specific statistics from the `StatisticsService` via an API endpoint:
```csharp
@inject HttpClient Http

protected override async Task OnInitializedAsync()
{
    var userId = "example-user-id"; 
    var statistics = await Http.GetFromJsonAsync<UserEmissionStatistics>($"api/statistics/{userId}");
    // Use statistics data to populate the dashboard
}
```

### **Downloading Reports**
The Reports page allows users to generate a CSV report for a specific period:
```csharp
@inject HttpClient Http

private async Task DownloadReport()
{
    var reportBytes = await Http.GetByteArrayAsync($"api/reports?userId=example-user-id&startDate=2023-01-01&endDate={DateTime.UtcNow}");
    var fileName = "Emissions_Report.csv";
    await FileUtil.SaveAs(fileName, reportBytes);
}
```
