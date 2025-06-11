using AutoMapper;
using FreelanceProjectBoardApi.DTOs.ClientProfiles;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Services.Interfaces;

namespace FreelanceProjectBoardApi.Services.Implementations
{
    public class ClientProfileService : IClientProfileService
    {
        private readonly IClientProfileRepository _clientProfileRepository;
        private readonly IUserRepository _userRepository; // Needed to verify user existence/type
        private readonly IMapper _mapper;

        public ClientProfileService(
            IClientProfileRepository clientProfileRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _clientProfileRepository = clientProfileRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ClientProfileResponseDto?> CreateClientProfileAsync(Guid userId, CreateClientProfileDto createDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || (user.Type != UserType.Client))
            {
                return null; // User not found or not a client type
            }

            // Check if profile already exists
            var existingProfile = await _clientProfileRepository.GetAllAsync(cp => cp.UserId == userId);
            if (existingProfile.Any())
            {
                return null; // Profile already exists for this user
            }

            var clientProfile = _mapper.Map<ClientProfile>(createDto);
            clientProfile.UserId = userId; // Linking to user
            clientProfile.User = user;

            await _clientProfileRepository.AddAsync(clientProfile);
            await _clientProfileRepository.SaveChangesAsync();

            return _mapper.Map<ClientProfileResponseDto>(clientProfile);
        }

        public async Task<ClientProfileResponseDto?> GetClientProfileByIdAsync(Guid id)
        {
            var profile = await _clientProfileRepository.GetClientProfileDetailsAsync(id);
            return profile == null ? null : _mapper.Map<ClientProfileResponseDto>(profile);
        }

        public async Task<ClientProfileResponseDto?> GetClientProfileByUserIdAsync(Guid userId)
        {
            var profile = await _clientProfileRepository.GetAllAsync(cp => cp.UserId == userId, includeProperties: "User,PostedProjects");
            return profile.FirstOrDefault() == null ? null : _mapper.Map<ClientProfileResponseDto>(profile.FirstOrDefault());
        }

        public async Task<ClientProfileResponseDto?> UpdateClientProfileAsync(Guid id, UpdateClientProfileDto updateDto)
        {
            var profile = await _clientProfileRepository.GetByIdAsync(id);
            if (profile == null)
            {
                return null;
            }

            _mapper.Map(updateDto, profile); // Applying updates

            await _clientProfileRepository.UpdateAsync(profile);
            await _clientProfileRepository.SaveChangesAsync();

            return _mapper.Map<ClientProfileResponseDto>(profile);
        }

        public async Task<bool> DeleteClientProfileAsync(Guid id)
        {
            var profile = await _clientProfileRepository.GetByIdAsync(id);
            if (profile == null)
            {
                return false;
            }

            await _clientProfileRepository.DeleteAsync(id);
            await _clientProfileRepository.SaveChangesAsync();
            return true;
        }

        public async Task<PageResult<ClientProfileResponseDto>> GetAllClientProfilesAsync(ClientFilter filter, PaginationParams pagination)
        {
            var pagedResult = await _clientProfileRepository.GetAllClientProfileAsync(filter, pagination);
            var profileDtos = _mapper.Map<IEnumerable<ClientProfileResponseDto>>(pagedResult.Data);

            return new PageResult<ClientProfileResponseDto>
            {
                Data = profileDtos,
                pagination = pagedResult.pagination
            };
        }
    }
}
