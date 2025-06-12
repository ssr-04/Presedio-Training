using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace FreelanceProjectBoardApi.Models
{
    public class Skill : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}