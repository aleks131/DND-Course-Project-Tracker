using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Backend.Services;
using Shared.Models;

namespace Backend.Controllers
{
    [Authorize]
    public class EmissionsController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly StatisticsService _statisticsService;

        public EmissionsController(
            ApplicationDbContext context, 
            NotificationService notificationService,
            StatisticsService statisticsService)
        {
            _context = context;
            _notificationService = notificationService;
            _statisticsService = statisticsService;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<List<Backend.Models.EmissionRecord>>> GetUserEmissions(string userId)
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
        public async Task<ActionResult<Backend.Models.EmissionRecord>> CreateEmission([FromBody] Shared.Models.EmissionRecord emission)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != emission.UserId)
            {
                return Forbid();
            }

            var dbEmission = Backend.Models.EmissionRecord.FromShared(emission);
            _context.EmissionRecords.Add(dbEmission);
            await _context.SaveChangesAsync();

            await _notificationService.NotifyEmissionCreated(emission.UserId, dbEmission);
            var statistics = await _statisticsService.GetUserStatisticsAsync(emission.UserId);
            await _notificationService.NotifyStatisticsUpdate(emission.UserId, statistics);

            return CreatedAtAction(nameof(GetUserEmissions), new { userId = emission.UserId }, dbEmission);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmission(int id, [FromBody] Shared.Models.EmissionRecord emission)
        {
            if (id != emission.Id)
                return BadRequest();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var existingEmission = await _context.EmissionRecords.FindAsync(id);
            if (existingEmission == null)
                return NotFound();

            if (existingEmission.UserId != userId)
                return Forbid();

            existingEmission.Date = emission.Date;
            existingEmission.Type = emission.Type;
            existingEmission.Source = emission.Source;
            existingEmission.Amount = emission.Amount;
            existingEmission.Description = emission.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EmissionRecordExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        private async Task<bool> EmissionRecordExists(int id)
        {
            return await _context.EmissionRecords.AnyAsync(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmission(string id)
        {
            var emission = await _context.EmissionRecords.FindAsync(id);
            if (emission == null)
                return NotFound();

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != emission.UserId)
                return Forbid();

            _context.EmissionRecords.Remove(emission);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("statistics/{userId}")]
        public async Task<ActionResult<UserEmissionStatistics>> GetStatistics(string userId)
        {
            var statistics = await _statisticsService.GetUserStatisticsAsync(userId);
            return Ok(statistics);
        }
    }
} 