using global::Shared.Models;

namespace Frontend.Services
{
    public interface IEmissionService
    {
        Task<List<EmissionRecord>> GetEmissionsAsync();
        Task<EmissionRecord> AddEmissionAsync(EmissionRecord emission);
        Task<bool> DeleteEmissionAsync(string id);
        Task<EmissionRecord> UpdateEmissionAsync(EmissionRecord emission);
    }
} 