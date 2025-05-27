using System.ComponentModel.DataAnnotations;

public class SpecialityUpdateDto
{
    [Required(ErrorMessage = "Id is required.")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(75, ErrorMessage = "Name cannot exceed 75 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required.")]
    [StringLength(30, ErrorMessage = "Status cannot exceed 30 characters.")]
    public string Status { get; set; } = string.Empty;
}