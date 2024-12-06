using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserEmissionStatistics> GetUserStatisticsAsync(string userId)
        {
            var emissions = await _context.EmissionRecords
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.Date)
                .ToListAsync();

            if (!emissions.Any())
                return new UserEmissionStatistics();

            var totalEmissions = emissions.Sum(e => (double)e.Amount);
            var monthlyAvg = CalculateMonthlyAverage(emissions);
            var dailyAvg = CalculateDailyAverage(emissions);
            var monthlyTrend = GetMonthlyTrend(emissions);

            var typeGroups = await GetEmissionsByTypeAsync(userId);
            var sourceGroups = await GetEmissionsBySourceAsync(userId);

            return new UserEmissionStatistics
            {
                TotalEmissions = totalEmissions,
                MonthlyAverage = monthlyAvg,
                DailyAverage = dailyAvg,
                MonthlyTrend = monthlyTrend,
                EmissionsByType = typeGroups.ToDictionary(x => x.Key, x => (double)x.Value),
                EmissionsBySource = sourceGroups.ToDictionary(x => x.Key, x => (double)x.Value)
            };
        }

        public async Task<EmissionSummary> GetUserDashboardDataAsync(string userId)
        {
            var statistics = await GetUserStatisticsAsync(userId);
            var recentEmissions = await _context.EmissionRecords
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Take(10)
                .ToListAsync();

            return new EmissionSummary
            {
                Statistics = statistics,
                RecentEmissions = recentEmissions
            };
        }

        private double CalculateMonthlyAverage(List<EmissionRecord> emissions)
        {
            if (!emissions.Any())
                return 0;

            var months = (emissions.Max(e => e.Date) - emissions.Min(e => e.Date)).Days / 30.0;
            return months > 0 ? (double)emissions.Sum(e => e.Amount) / months : 0;
        }

        private double CalculateDailyAverage(List<EmissionRecord> emissions)
        {
            if (!emissions.Any())
                return 0;

            var days = (emissions.Max(e => e.Date) - emissions.Min(e => e.Date)).Days;
            return days > 0 ? (double)emissions.Sum(e => e.Amount) / days : 0;
        }

        private List<MonthlyEmission> GetMonthlyTrend(List<EmissionRecord> emissions)
        {
            return emissions
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new MonthlyEmission
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Total = (double)g.Sum(e => e.Amount)
                })
                .OrderBy(m => m.Month)
                .ToList();
        }

        private async Task<Dictionary<string, decimal>> GetEmissionsByTypeAsync(string userId)
        {
            var emissions = await _context.EmissionRecords
                .Where(e => e.UserId == userId)
                .GroupBy(e => e.Type)
                .Select(g => new { Type = g.Key, Total = (decimal)g.Sum(e => e.Amount) })
                .ToDictionaryAsync(x => x.Type, x => x.Total);

            return emissions;
        }

        private async Task<Dictionary<string, decimal>> GetEmissionsBySourceAsync(string userId)
        {
            var emissions = await _context.EmissionRecords
                .Where(e => e.UserId == userId)
                .GroupBy(e => e.Source)
                .Select(g => new { Source = g.Key, Total = (decimal)g.Sum(e => e.Amount) })
                .ToDictionaryAsync(x => x.Source, x => x.Total);

            return emissions;
        }
    }
} 