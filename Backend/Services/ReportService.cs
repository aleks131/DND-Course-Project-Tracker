using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using CsvHelper;
using System.Globalization;

namespace Backend.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateEmissionsReportAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var emissions = await _context.EmissionRecords
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .OrderBy(e => e.Date)
                .ToListAsync();

            // Generate report logic here
            // For now, return a simple CSV format
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
} 