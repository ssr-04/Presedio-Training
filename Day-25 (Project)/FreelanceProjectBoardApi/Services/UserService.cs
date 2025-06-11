using AutoMapper;
using FreelanceProjectBoardApi.DTOs.Users;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Services.Interfaces;

namespace FreelanceProjectBoardApi.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            //System.Console.WriteLine(user.ClientProfile.Id);
            return user == null ? null : _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            return user == null ? null : _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto?> UpdateUserAsync(Guid id, UpdateUserDto updateDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            _mapper.Map(updateDto, user); // Appling updates from DTO to entity

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            await _userRepository.DeleteAsync(id); // Soft delete
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<PageResult<UserResponseDto>> GetAllUsersAsync(UserFilter filter, PaginationParams pagination)
        {
            // repository handles the filtering, sorting, and pagination logic
            var pagedResult = await _userRepository.GetAllUsersAsync(filter, pagination, includeProfiles: true);

            // Mapping the list of User models to UserResponseDto
            var userDtos = _mapper.Map<IEnumerable<UserResponseDto>>(pagedResult.Data);
            
            return new PageResult<UserResponseDto>
            {
                Data = userDtos,
                pagination = pagedResult.pagination
            };
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                return false; // User not found or current password incorrect
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }
    }
}