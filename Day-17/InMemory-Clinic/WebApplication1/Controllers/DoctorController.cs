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

    // GET: api/Doctors
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetDoctors()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        return Ok(doctors);
    }

    // GET: api/Doctors/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorResponseDto>> GetDoctor(Guid id)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id);
        if (doctor == null)
        {
            return NotFound();
        }
        return Ok(doctor);
    }

    // POST: api/Doctors
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DoctorResponseDto>> PostDoctor([FromBody] DoctorCreateDto doctorDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (addedDoctor, error) = await _doctorService.AddDoctorAsync(doctorDto);
        if (addedDoctor == null)
        {
            return BadRequest(error);
        }
        return CreatedAtAction(nameof(GetDoctor), new { id = addedDoctor.Id }, addedDoctor);
    }

    // PUT: api/Doctors/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorResponseDto>> PutDoctor(Guid id, [FromBody] DoctorUpdateDto doctorDto)
    {
        if (id != doctorDto.Id)
        {
            return BadRequest("ID in URL does not match ID in request body.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (updatedDoctor, error) = await _doctorService.UpdateDoctorAsync(doctorDto);
        if (updatedDoctor == null)
        {
            if (error == "Doctor not found.")
            {
                return NotFound(error);
            }
            return BadRequest(error);
        }
        return Ok(updatedDoctor);
    }

    // DELETE: api/Doctors/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteDoctor(Guid id)
    {
        var doctorExists = await _doctorService.GetDoctorByIdAsync(id) != null;
        if (!doctorExists)
        {
            return NotFound();
        }

        var deleted = await _doctorService.DeleteDoctorAsync(id);
        if (!deleted)
        {
            return BadRequest("Something went wrong.");
        }
        return NoContent();
    }
}