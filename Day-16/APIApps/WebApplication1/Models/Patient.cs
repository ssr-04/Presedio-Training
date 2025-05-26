using System;
using System.ComponentModel.DataAnnotations; 

public class Patient
{
    //Just some validation check
    [Required(ErrorMessage = "Patient ID is required.")]
    [Range(101, int.MaxValue, ErrorMessage = "Patient ID must be positive.")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Patient name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Patient name must be between 2 and 100 characters.")]
    public string Name { get; set; } = String.Empty;

    [Required(ErrorMessage = "Date of Birth is required.")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Ailment is required.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Ailment description must be between 5 and 200 characters.")]
    public string Ailment { get; set; } = String.Empty;

}
