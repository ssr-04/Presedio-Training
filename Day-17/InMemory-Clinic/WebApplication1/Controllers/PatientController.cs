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

    // GET: api/Patients/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PatientResponseDto>>> GetPatients()
    {
        var patients = await _patientService.GetAllPatientsAsync();
        return Ok(patients);
    }

    // GET: api/Patients/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientResponseDto>> GetPatient(Guid id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null)
        {
            return NotFound();
        }
        return Ok(patient);
    }

    // POST: api/Patients
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatientResponseDto>> PostPatient([FromBody] PatientCreateDto patientDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (addedPatient, error) = await _patientService.AddPatientAsync(patientDto);
        if (addedPatient == null)
        {
            return BadRequest(error);
        }
        return CreatedAtAction(nameof(GetPatient), new { id = addedPatient.Id }, addedPatient);
    }

    // PUT: api/Patients/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientResponseDto>> PutPatient(Guid id, [FromBody] PatientUpdateDto patientDto)
    {
        if (id != patientDto.Id)
        {
            return BadRequest("ID in URL does not match ID in request body.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (updatedPatient, error) = await _patientService.UpdatePatientAsync(patientDto);
        if (updatedPatient == null)
        {
            if (error == "Patient not found.")
            {
                return NotFound(error);
            }
            return BadRequest(error);
        }
        return Ok(updatedPatient);
    }

    // DELETE: api/Patients/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // for future potential business rule violations
    public async Task<IActionResult> DeletePatient(Guid id)
    {
        var patientExists = await _patientService.GetPatientByIdAsync(id) != null;
        if (!patientExists)
        {
            return NotFound();
        }

        var deleted = await _patientService.DeletePatientAsync(id);
        if (!deleted)
        {
            return BadRequest("Something went wrong");
        }
        return NoContent();
    }
}