using AutoMapper;
using FreelanceProjectBoardApi.DTOs.Skills;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Services.Interfaces;

namespace FreelanceProjectBoardApi.Services.Implementations
{
    public class SkillService : ISkillService
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IMapper _mapper;

        public SkillService(ISkillRepository skillRepository, IMapper mapper)
        {
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public async Task<SkillDto?> CreateSkillAsync(CreateSkillDto createDto)
        {
            // Ensures skill name is unique (case-insensitive)
            var existingSkill = await _skillRepository.GetSkillByNameAsync(createDto.Name);
            if (existingSkill != null)
            {
                return null; // Skill with this name already exists
            }

            var skill = _mapper.Map<Skill>(createDto);
            await _skillRepository.AddAsync(skill);
            await _skillRepository.SaveChangesAsync();

            return _mapper.Map<SkillDto>(skill);
        }

        public async Task<SkillDto?> GetSkillByIdAsync(Guid id)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            return skill == null ? null : _mapper.Map<SkillDto>(skill);
        }

        public async Task<SkillDto?> GetSkillByNameAsync(string name)
        {
            var skill = await _skillRepository.GetSkillByNameAsync(name);
            return skill == null ? null : _mapper.Map<SkillDto>(skill);
        }

        public async Task<PageResult<SkillDto>> GetAllSkillsAsync(SkillFilter filter, PaginationParams pagination)
        {
            var skillsResult = await _skillRepository.GetAllSkillsAsync(filter, pagination);
            var skillsDto =  _mapper.Map<IEnumerable<SkillDto>>(skillsResult.Data);

            return new PageResult<SkillDto>
            {
                Data = skillsDto,
                pagination = skillsResult.pagination
            };
        }

        public async Task<bool> DeleteSkillAsync(Guid id)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill == null)
            {
                return false;
            }
            await _skillRepository.DeleteAsync(id); // Soft delete
            await _skillRepository.SaveChangesAsync();
            return true;
        }
    }
}
