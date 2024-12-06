using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend.Services;
using Backend.Models;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Authorize]
    public class StatisticsController : BaseApiController
    {
        private readonly StatisticsService _statisticsService;

        public StatisticsController(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("dashboard/{userId}")]
        public async Task<ActionResult<EmissionSummary>> GetDashboardData(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != userId)
                return Forbid();

            var summary = await _statisticsService.GetUserDashboardDataAsync(userId);
            return Ok(summary);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<UserEmissionStatistics>> GetUserStatistics(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != userId)
                return Forbid();

            var statistics = await _statisticsService.GetUserStatisticsAsync(userId);
            return Ok(statistics);
        }
    }
}
