using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SpecialitiesController : ControllerBase
{
    private readonly ISpecialityService _specialityService;

    public SpecialitiesController(ISpecialityService specialityService)
    {
        _specialityService = specialityService;
    }

    // GET: api/specialities
    [HttpGet]
    [AllowAnonymous] //anyone can access (even unauthenticated)
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SpecialityResponseDto>>> GetSpecialities([FromQuery] bool includeDeleted = false)
    {
        var specialities = await _specialityService.GetAllSpecialitiesAsync(includeDeleted);
        return Ok(specialities);
    }

    // GET: api/specialities/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Doctor,Patient,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpecialityResponseDto>> GetSpeciality(int id, [FromQuery] bool includeDeleted = false)
    {
        var speciality = await _specialityService.GetSpecialityByIdAsync(id, includeDeleted);
        if (speciality == null)
        {
            return NotFound($"Speciality with ID {id} not found.");
        }
        return Ok(speciality);
    }

    // POST: api/specialities
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SpecialityResponseDto>> PostSpeciality([FromBody] SpecialityCreateDto specialityDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdSpeciality = await _specialityService.AddSpecialityAsync(specialityDto);
            return CreatedAtAction(nameof(GetSpeciality), new { id = createdSpeciality.Id }, createdSpeciality);
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message);
        }
    }

    // PUT: api/specialities/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SpecialityResponseDto>> PutSpeciality(int id, [FromBody] SpecialityUpdateDto specialityDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedSpeciality = await _specialityService.UpdateSpecialityAsync(id, specialityDto);
            if (updatedSpeciality == null)
            {
                return NotFound($"Speciality with ID {id} not found.");
            }
            return Ok(updatedSpeciality);
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message);
        }
    }

    // DELETE: api/specialities/{id} (Soft Delete)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteSpeciality(int id)
    {
        try
        {
            var deleted = await _specialityService.SoftDeleteSpecialityAsync(id);
            if (!deleted)
            {
                return NotFound($"Speciality with ID {id} not found or already deleted.");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message); // Cannot delete speciality with active doctor associations
        }
    }
}