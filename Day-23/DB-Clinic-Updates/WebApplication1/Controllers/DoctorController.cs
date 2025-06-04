
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly IAuthorizationService _authorizationService;

    public DoctorsController(IDoctorService doctorService, IAuthorizationService authorizationService)
    {
        _doctorService = doctorService;
        _authorizationService = authorizationService;
    }

    // GET: api/doctors
    [HttpGet]
    [Authorize(Roles = "Doctor,Patient,Admin")] // All authenticated users can view
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetDoctors([FromQuery] bool includeDeleted = false)
    {
        var doctors = await _doctorService.GetAllDoctorsAsync(includeDeleted);
        return Ok(doctors);
    }

    // GET: api/doctors/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Doctor,Patient,Admin")] // all authenticated users can
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
    [Authorize(Roles = "Admin")] //Only admin can add new doctors
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
    [Authorize(Roles = "Doctor,Admin")] // Doctor can update their own, Admin any
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

        var existingDoctor = await _doctorService.GetDoctorByIdAsync(id, includeDeleted: true);
        if (existingDoctor == null)
        {
            return NotFound($"Doctor with ID {id} not found.");
        }

        // Policy-based authorization: Doctor can only update their own record
        if (User.IsInRole("Doctor"))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, existingDoctor, ResourceOperations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }
        // Admin role is covered by Role based so they can update too

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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Doctor,Patient,Admin")] // Any authorised user
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