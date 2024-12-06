using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend.Services;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Authorize]
    public class ReportsController : BaseApiController
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("emissions/csv")]
        public async Task<IActionResult> GetEmissionsCsv(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var reportBytes = await _reportService.GenerateEmissionsReportAsync(userId, startDate, endDate);
            return File(
                reportBytes,
                "text/csv",
                $"emissions_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv"
            );
        }
    }
} 