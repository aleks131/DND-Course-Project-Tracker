using Backend.Models;

namespace Backend.Services
{
    public interface IStatisticsService
    {
        Task<UserEmissionStatistics> GetUserStatisticsAsync(string userId);
        Task<EmissionSummary> GetUserDashboardDataAsync(string userId);
    }
} 