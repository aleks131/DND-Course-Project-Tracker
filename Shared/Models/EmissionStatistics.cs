namespace Shared.Models
{
    public class EmissionStatistics
    {
        public double TotalEmissions { get; set; }
        public double MonthlyAverage { get; set; }
        public double DailyAverage { get; set; }
        public IDictionary<string, double> EmissionsByType { get; set; } = new Dictionary<string, double>();
        public IDictionary<string, double> EmissionsBySource { get; set; } = new Dictionary<string, double>();
        public List<MonthlyData> MonthlyData { get; set; } = new();
    }

    public class MonthlyData
    {
        public DateTime Month { get; set; }
        public double Total { get; set; }
    }
} 