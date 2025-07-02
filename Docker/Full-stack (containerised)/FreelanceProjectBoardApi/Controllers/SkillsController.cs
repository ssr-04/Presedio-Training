using Asp.Versioning;
using FreelanceProjectBoardApi.DTOs.Skills;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Models; // For UserType
using FreelanceProjectBoardApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace FreelanceProjectBoardApi.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    public class SkillsController : BaseApiController
    {
        private readonly ISkillService _skillService;

        public SkillsController(ISkillService skillService)
        {
            _skillService = skillService;
        }


        [HttpPost]
        [Authorize(Roles = "Admin")] // Restricts skill creation to Admin
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SkillDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateSkill([FromBody] CreateSkillDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var skill = await _skillService.CreateSkillAsync(createDto);
            if (skill == null)
            {
                return Conflict(new { message = "Skill with this name already exists." });
            }
            return CreatedAtAction(nameof(GetSkillById), new { id = skill.Id }, skill);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SkillDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSkillById(Guid id)
        {
            var skill = await _skillService.GetSkillByIdAsync(id);
            if (skill == null)
            {
                return NotFound($"Skill with ID {id} not found.");
            }
            return Ok(skill);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageResult<SkillDto>))]
        public async Task<IActionResult> GetAllSkills([FromQuery] SkillFilter filter, [FromQuery] PaginationParams pagination)
        {
            var skills = await _skillService.GetAllSkillsAsync(filter, pagination);
            return Ok(skills);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Restrict skill deletion to Admin
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSkill(Guid id)
        {
            var success = await _skillService.DeleteSkillAsync(id);
            if (!success)
            {
                return NotFound($"Skill with ID {id} not found.");
            }
            return NoContent();
        }
    }
}
