using AutoMapper;
using FreelanceProjectBoardApi.DTOs.FreelancerProfiles;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Services.Interfaces;

namespace FreelanceProjectBoardApi.Services.Implementations
{
    public class FreelancerProfileService : IFreelancerProfileService
    {
        private readonly IFreelancerProfileRepository _freelancerProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IFreelancerSkillRepository _freelancerSkillRepository;
        private readonly IFileService _fileService; // To handle actual file uploads/downloads
        private readonly IRatingRepository _ratingRepository; // To calculate average rating
        private readonly IMapper _mapper;

        public FreelancerProfileService(
            IFreelancerProfileRepository freelancerProfileRepository,
            IUserRepository userRepository,
            ISkillRepository skillRepository,
            IFreelancerSkillRepository freelancerSkillRepository,
            IFileService fileService,
            IRatingRepository ratingRepository,
            IMapper mapper)
        {
            _freelancerProfileRepository = freelancerProfileRepository;
            _userRepository = userRepository;
            _skillRepository = skillRepository;
            _freelancerSkillRepository = freelancerSkillRepository;
            _fileService = fileService;
            _ratingRepository = ratingRepository;
            _mapper = mapper;
        }

        public async Task<FreelancerProfileResponseDto?> CreateFreelancerProfileAsync(Guid userId, CreateFreelancerProfileDto createDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || (user.Type != UserType.Freelancer))
            {
                return null; // User not found or not a freelancer type
            }

            var existingProfile = await _freelancerProfileRepository.GetAllAsync(fp => fp.UserId == userId);
            if (existingProfile.Any())
            {
                return null; // Profile already exists for this user
            }

            var freelancerProfile = _mapper.Map<FreelancerProfile>(createDto);
            freelancerProfile.UserId = userId;

            freelancerProfile = await _freelancerProfileRepository.AddAsync(freelancerProfile);
            await _freelancerProfileRepository.SaveChangesAsync();

            // Handling skills: Find existing or create new skills, then add associations
            if (createDto.Skills != null && createDto.Skills.Any())
            {
                var freelancerSkillsToAdd = new List<FreelancerSkill>();
                foreach (var skill in createDto.Skills)
                {
                    var existingSkill = await _skillRepository.GetSkillByNameAsync(skill.Name);
                    if (existingSkill == null)
                    {
                        existingSkill = await _skillRepository.AddAsync(_mapper.Map<Skill>(skill));
                    }

                    freelancerSkillsToAdd.Add(new FreelancerSkill
                    {
                        FreelancerProfileId = freelancerProfile.Id,
                        SkillId = existingSkill.Id,
                        FreelancerProfile = freelancerProfile,
                        Skill = existingSkill
                    });
                }
                // Add all freelancer skills at once
                await _freelancerSkillRepository.AddRangeAsync(freelancerSkillsToAdd);
                await _freelancerSkillRepository.SaveChangesAsync();
            }

            var createdProfile = await _freelancerProfileRepository.GetFreelancerProfileDetailsAsync(freelancerProfile.Id);
            if (createdProfile == null) return null; // Should not happen if AddAsync was successful

            var dto = _mapper.Map<FreelancerProfileResponseDto>(createdProfile);
            return dto;
        }

        public async Task<FreelancerProfileResponseDto?> GetFreelancerProfileByIdAsync(Guid id)
        {
            var profile = await _freelancerProfileRepository.GetFreelancerProfileDetailsAsync(id);
            if (profile == null) return null;

            var dto = _mapper.Map<FreelancerProfileResponseDto>(profile);
            dto.AverageRating = await _ratingRepository.GetAverageRatingForUserAsync(profile.UserId); // Calculate average rating
            return dto;
        }

        public async Task<FreelancerProfileResponseDto?> GetFreelancerProfileByUserIdAsync(Guid userId)
        {
            var profile = await _freelancerProfileRepository.GetAllAsync(
                fp => fp.UserId == userId,
                includeProperties: "User,FreelancerSkills.Skill,ResumeFile,ProfilePictureFile,RatingsAsRatee"
            );
            var foundProfile = profile.FirstOrDefault();

            if (foundProfile == null) return null;

            var dto = _mapper.Map<FreelancerProfileResponseDto>(foundProfile);
            dto.AverageRating = await _ratingRepository.GetAverageRatingForUserAsync(userId); // Calculate average rating
            return dto;
        }

        public async Task<FreelancerProfileResponseDto?> UpdateFreelancerProfileAsync(Guid id, UpdateFreelancerProfileDto updateDto)
        {
            var profile = await _freelancerProfileRepository.GetFreelancerProfileDetailsAsync(id); // Loads with skills
            if (profile == null)
            {
                return null;
            }

            _mapper.Map(updateDto, profile);

            // Updating skills
            if (updateDto.Skills != null && updateDto.Skills.Any())
            {
                await _freelancerSkillRepository.DeleteFreelancerSkills(id);
                var freelancerSkillsToAdd = new List<FreelancerSkill>();
                foreach (var skill in updateDto.Skills)
                {
                    var existingSkill = await _skillRepository.GetSkillByNameAsync(skill.Name);
                    
                    if (existingSkill == null)
                        {
                            existingSkill = await _skillRepository.AddAsync(_mapper.Map<Skill>(skill));
                        }
                    
                    freelancerSkillsToAdd.Add(new FreelancerSkill
                    {
                        FreelancerProfileId = id,
                        SkillId = existingSkill.Id,
                        FreelancerProfile = profile,
                        Skill = existingSkill
                    });
                }
                // Add all freelancer skills at once
                await _freelancerSkillRepository.AddRangeAsync(freelancerSkillsToAdd);
                await _freelancerSkillRepository.SaveChangesAsync();
            }

            await _freelancerProfileRepository.UpdateAsync(profile);
            await _freelancerProfileRepository.SaveChangesAsync();

            var updatedProfile = await _freelancerProfileRepository.GetFreelancerProfileDetailsAsync(profile.Id); // Reload to get latest skills
            var dto = _mapper.Map<FreelancerProfileResponseDto>(updatedProfile);
            dto.AverageRating = await _ratingRepository.GetAverageRatingForUserAsync(profile.UserId);
            return dto;
        }

        public async Task<bool> DeleteFreelancerProfileAsync(Guid id)
        {
            var profile = await _freelancerProfileRepository.GetByIdAsync(id);
            if (profile == null)
            {
                return false;
            }

            await _freelancerProfileRepository.DeleteAsync(id);
            await _freelancerProfileRepository.SaveChangesAsync();
            return true;
        }

        public async Task<PageResult<FreelancerProfileResponseDto>> GetAllFreelancerProfilesAsync(FreelancerFilter filter, PaginationParams pagination)
        {
            var pagedResult = await _freelancerProfileRepository.GetAllFreelancerProfilesAsync(filter, pagination);
            var profileDtos = _mapper.Map<IEnumerable<FreelancerProfileResponseDto>>(pagedResult.Data);

            // Populate AverageRating for each DTO in the paged result
            foreach (var dto in profileDtos)
            {
                // This might be N+1 queries if not optimized; consider loading ratings with main query
                // or pre-calculating and storing average rating if performance is critical for large lists.
                dto.AverageRating = await _ratingRepository.GetAverageRatingForUserAsync(dto.UserId);
            }

            return new PageResult<FreelancerProfileResponseDto>
            {
                Data = profileDtos,
                pagination = pagedResult.pagination
            };
        }

        public async Task<FreelancerProfileResponseDto?> UploadResumeAsync(Guid freelancerProfileId, IFormFile file)
        {
            var profile = await _freelancerProfileRepository.GetByIdAsync(freelancerProfileId);
            if (profile == null) return null;

            var uploadedFileDto = await _fileService.UploadFileAsync(file, profile.UserId, FileCategory.Resume, freelancerProfileId);
            if (uploadedFileDto == null) return null;

            profile.ResumeFileId = uploadedFileDto.Id;
            await _freelancerProfileRepository.UpdateAsync(profile);
            await _freelancerProfileRepository.SaveChangesAsync();

            return await GetFreelancerProfileByIdAsync(freelancerProfileId); // Reloads with updated file info
        }

        public async Task<FreelancerProfileResponseDto?> UploadProfilePictureAsync(Guid freelancerProfileId, IFormFile file)
        {
            var profile = await _freelancerProfileRepository.GetByIdAsync(freelancerProfileId);
            if (profile == null) return null;

            var uploadedFileDto = await _fileService.UploadFileAsync(file, profile.UserId, FileCategory.ProfilePicture, freelancerProfileId);
            if (uploadedFileDto == null) return null;

            profile.ProfilePictureFileId = uploadedFileDto.Id;
            await _freelancerProfileRepository.UpdateAsync(profile);
            await _freelancerProfileRepository.SaveChangesAsync();

            return await GetFreelancerProfileByIdAsync(freelancerProfileId);
        }

        public async Task<bool> RemoveResumeAsync(Guid freelancerProfileId)
        {
            var profile = await _freelancerProfileRepository.GetByIdAsync(freelancerProfileId);
            if (profile == null || profile.ResumeFileId == null) return false;

            var fileId = profile.ResumeFileId.Value;
            profile.ResumeFileId = null; // Detach from profile
            await _freelancerProfileRepository.UpdateAsync(profile);
            await _freelancerProfileRepository.SaveChangesAsync();

            return await _fileService.DeleteFileAsync(fileId); // Deletes file metadata and actual file
        }

        public async Task<bool> RemoveProfilePictureAsync(Guid freelancerProfileId)
        {
            var profile = await _freelancerProfileRepository.GetByIdAsync(freelancerProfileId);
            if (profile == null || profile.ProfilePictureFileId == null) return false;

            var fileId = profile.ProfilePictureFileId.Value;
            profile.ProfilePictureFileId = null; // Detach from profile
            await _freelancerProfileRepository.UpdateAsync(profile);
            await _freelancerProfileRepository.SaveChangesAsync();

            return await _fileService.DeleteFileAsync(fileId);
        }


        private async Task UpdateSkillsForFreelancerProfile(Guid freelancerProfileId, List<Guid> newSkillIds)
        {
            var currentFreelancerSkills = (await _freelancerSkillRepository.GetSkillsForFreelancerAsync(freelancerProfileId)).ToList();
            var currentSkillIds = currentFreelancerSkills.Select(fs => fs.SkillId).ToList();

            // Skills to add
            var skillsToAdd = newSkillIds.Except(currentSkillIds).ToList();
            System.Console.WriteLine($"Hit\n\n\n {currentSkillIds[0]} {newSkillIds[0]} {skillsToAdd[1]} The end\n\n\n");
            foreach (var skillId in skillsToAdd)
            {
                var skillExists = await _skillRepository.GetByIdAsync(skillId);
                if (skillExists != null)
                {
                    await _freelancerSkillRepository.AddAsync(new FreelancerSkill
                    {
                        FreelancerProfileId = freelancerProfileId,
                        SkillId = skillId,
                        FreelancerProfile = (await _freelancerProfileRepository.GetByIdAsync(freelancerProfileId))!,
                        Skill = skillExists

                    });
                }
            }

            // Skills to remove (soft delete)
            var skillsToRemove = currentSkillIds.Except(newSkillIds).ToList();
            foreach (var skillId in skillsToRemove)
            {
                var freelancerSkillToDelete = currentFreelancerSkills.FirstOrDefault(fs => fs.SkillId == skillId);
                if (freelancerSkillToDelete != null)
                {
                    await _freelancerSkillRepository.DeleteAsync(freelancerSkillToDelete.Id); // Soft delete the join entry
                }
            }
        }
    }
}
