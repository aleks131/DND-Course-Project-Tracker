using Backend.Models;

namespace Backend.Models
{
    public class UserEmissionStatistics
    {
        public double TotalEmissions { get; set; }
        public double MonthlyAverage { get; set; }
        public double DailyAverage { get; set; }
        public IDictionary<string, double> EmissionsByType { get; set; } = new Dictionary<string, double>();
        public IDictionary<string, double> EmissionsBySource { get; set; } = new Dictionary<string, double>();
        public List<MonthlyEmission> MonthlyTrend { get; set; } = new();
    }

    public class MonthlyEmission
    {
        public DateTime Month { get; set; }
        public double Total { get; set; }
    }

    public class EmissionSummary
    {
        public UserEmissionStatistics Statistics { get; set; } = new();
        public List<Backend.Models.EmissionRecord> RecentEmissions { get; set; } = new();
    }

    public class EmissionTrend
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DataPoint> Points { get; set; } = new();
        public double TotalEmissions { get; set; }
        public double ChangePercentage { get; set; }
    }

    public class DataPoint
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class EmissionComparison
    {
        public double CurrentPeriodTotal { get; set; }
        public double PreviousPeriodTotal { get; set; }
        public double ChangePercentage { get; set; }
        public Dictionary<string, double> BySource { get; set; } = new();
        public Dictionary<string, double> ByType { get; set; } = new();
    }
} 