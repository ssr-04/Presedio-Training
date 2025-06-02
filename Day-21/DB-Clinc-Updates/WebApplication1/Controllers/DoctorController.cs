
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    // GET: api/doctors
    [HttpGet]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetDoctors([FromQuery] bool includeDeleted = false)
    {
        var doctors = await _doctorService.GetAllDoctorsAsync(includeDeleted);
        return Ok(doctors);
    }

    // GET: api/doctors/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorResponseDto>> GetDoctor(int id, [FromQuery] bool includeDeleted = false)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id, includeDeleted);
        if (doctor == null)
        {
            return NotFound($"Doctor with ID {id} not found.");
        }
        return Ok(doctor);
    }

    // POST: api/doctors
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DoctorResponseDto>> PostDoctor([FromBody] DoctorCreateDto doctorDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdDoctor = await _doctorService.AddDoctorAsync(doctorDto);
            return CreatedAtAction(nameof(GetDoctor), new { id = createdDoctor.Id }, createdDoctor);
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message);
        }
    }

    // PUT: api/doctors/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DoctorResponseDto>> PutDoctor(int id, [FromBody] DoctorUpdateDto doctorDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedDoctor = await _doctorService.UpdateDoctorAsync(id, doctorDto);
            if (updatedDoctor == null)
            {
                return NotFound($"Doctor with ID {id} not found.");
            }
            return Ok(updatedDoctor);
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message);
        }
    }

    // DELETE: api/doctors/{id} (Soft Delete)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        try
        {
            var deleted = await _doctorService.SoftDeleteDoctorAsync(id);
            if (!deleted)
            {
                return NotFound($"Doctor with ID {id} not found or already deleted.");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message); // like Cannot delete doctor with future appointments
        }
    }

    [HttpGet("bySpeciality/{specialityName}")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // If speciality not found
    public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetDoctorsBySpeciality(string specialityName)
    {
        try
        {
            var doctors = await _doctorService.GetDoctorsBySpecialityNameAsync(specialityName);
            return Ok(doctors);
        }
        catch (Exception ex) 
        {
            return BadRequest(ex.Message);
        }
    }
}