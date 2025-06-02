using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    // GET: api/patients
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PatientResponseDto>>> GetPatients([FromQuery] bool includeDeleted = false)
    {
        var patients = await _patientService.GetAllPatientsAsync(includeDeleted);
        return Ok(patients);
    }

    // GET: api/patients/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientResponseDto>> GetPatient(int id, [FromQuery] bool includeDeleted = false)
    {
        var patient = await _patientService.GetPatientByIdAsync(id, includeDeleted);
        if (patient == null)
        {
            return NotFound($"Patient with ID {id} not found.");
        }
        return Ok(patient);
    }

    // POST: api/patients
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // For email conflict
    public async Task<ActionResult<PatientResponseDto>> PostPatient([FromBody] PatientCreateDto patientDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdPatient = await _patientService.AddPatientAsync(patientDto);
            // 201 Created with the location of the newly created resource
            return CreatedAtAction(nameof(GetPatient), new { id = createdPatient.Id }, createdPatient);
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message); // 409 Conflict for cases like duplicate email
        }
    }

    // PUT: api/patients/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // For email conflict
    public async Task<ActionResult<PatientResponseDto>> PutPatient(int id, [FromBody] PatientUpdateDto patientDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedPatient = await _patientService.UpdatePatientAsync(id, patientDto);
            if (updatedPatient == null)
            {
                return NotFound($"Patient with ID {id} not found.");
            }
            return Ok(updatedPatient);
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message);
        }
    }

    // DELETE: api/patients/{id} (Soft Delete)
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // For business logic constraints
    public async Task<IActionResult> DeletePatient(int id)
    {
        try
        {
            var deleted = await _patientService.SoftDeletePatientAsync(id);
            if (!deleted)
            {
                return NotFound($"Patient with ID {id} not found or already deleted.");
            }
            return NoContent(); // 204 No Content for successful deletion
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message); // e.g., Cannot delete patient with future appointments
        }
    }
}