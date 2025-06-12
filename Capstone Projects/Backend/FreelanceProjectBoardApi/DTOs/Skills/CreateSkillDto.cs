using System.ComponentModel.DataAnnotations;

namespace FreelanceProjectBoardApi.DTOs.Skills
{
    public class CreateSkillDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
