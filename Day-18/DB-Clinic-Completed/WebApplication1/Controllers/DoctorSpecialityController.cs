using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DoctorSpecialitiesController : ControllerBase
{
    private readonly IDoctorSpecialityService _doctorSpecialityService;

    public DoctorSpecialitiesController(IDoctorSpecialityService doctorSpecialityService)
    {
        _doctorSpecialityService = doctorSpecialityService;
    }

    // GET: api/doctorspecialities
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DoctorSpecialityResponseDto>>> GetDoctorSpecialities()
    {
        var doctorSpecialities = await _doctorSpecialityService.GetAllDoctorSpecialitiesAsync();
        return Ok(doctorSpecialities);
    }

    // GET: api/doctorspecialities/{serialNumber}
    [HttpGet("{serialNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorSpecialityResponseDto>> GetDoctorSpeciality(int serialNumber)
    {
        var doctorSpeciality = await _doctorSpecialityService.GetDoctorSpecialityBySerialNumberAsync(serialNumber);
        if (doctorSpeciality == null)
        {
            return NotFound($"Doctor-Speciality association with Serial Number {serialNumber} not found.");
        }
        return Ok(doctorSpeciality);
    }

    // GET: api/doctorspecialities/byDoctor/{doctorId}
    [HttpGet("byDoctor/{doctorId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DoctorSpecialityResponseDto>>> GetDoctorSpecialitiesByDoctorId(int doctorId)
    {
        var doctorSpecialities = await _doctorSpecialityService.GetDoctorSpecialitiesByDoctorIdAsync(doctorId);
        return Ok(doctorSpecialities);
    }

    // GET: api/doctorspecialities/bySpeciality/{specialityId}
    [HttpGet("bySpeciality/{specialityId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DoctorSpecialityResponseDto>>> GetDoctorSpecialitiesBySpecialityId(int specialityId)
    {
        var doctorSpecialities = await _doctorSpecialityService.GetDoctorSpecialitiesBySpecialityIdAsync(specialityId);
        return Ok(doctorSpecialities);
    }

    // POST: api/doctorspecialities
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // For invalid IDs or duplicate association
    public async Task<ActionResult<DoctorSpecialityResponseDto>> PostDoctorSpeciality([FromBody] DoctorSpecialityCreateDto doctorSpecialityDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdAssociation = await _doctorSpecialityService.AddDoctorSpecialityAsync(doctorSpecialityDto);
            return CreatedAtAction(nameof(GetDoctorSpeciality), new { serialNumber = createdAssociation.SerialNumber }, createdAssociation);
        }
        catch (ArgumentException ex) // For Doctor/Speciality not found
        {
            return BadRequest(ex.Message); // foreign keys are invalid
        }
        catch (Exception ex) // For duplicate association
        {
            return Conflict(ex.Message); // 409 Conflict if already exists
        }
    }

    // DELETE: api/doctorspecialities/{serialNumber}
    [HttpDelete("{serialNumber}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDoctorSpeciality(int serialNumber)
    {
        var removed = await _doctorSpecialityService.RemoveDoctorSpecialityAsync(serialNumber);
        if (!removed)
        {
            return NotFound($"Doctor-Speciality association with Serial Number {serialNumber} not found.");
        }
        return NoContent();
    }
}