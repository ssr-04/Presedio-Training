using FreelanceProjectBoardApi.DTOs.ClientProfiles;
using FreelanceProjectBoardApi.Helpers;

namespace FreelanceProjectBoardApi.Services.Interfaces
{
    public interface IClientProfileService
    {
        Task<ClientProfileResponseDto?> CreateClientProfileAsync(Guid userId, CreateClientProfileDto createDto);
        Task<ClientProfileResponseDto?> GetClientProfileByIdAsync(Guid id);
        Task<ClientProfileResponseDto?> GetClientProfileByUserIdAsync(Guid userId);
        Task<ClientProfileResponseDto?> UpdateClientProfileAsync(Guid id, UpdateClientProfileDto updateDto);
        Task<bool> DeleteClientProfileAsync(Guid id);
        Task<PageResult<ClientProfileResponseDto>> GetAllClientProfilesAsync(ClientFilter filter, PaginationParams pagination);
    }
}
