using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IAuthorizationService _authorizationService; // Injecting IAuthorizationService for policy based


    public PatientsController(IPatientService patientService, IAuthorizationService authorizationService)
    {
        _patientService = patientService;
        _authorizationService = authorizationService;
    }

    // GET: api/patients
    [HttpGet]
    [Authorize(Roles = "Doctor,Admin")] // Only Doctors and Admins can view all patients
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PatientResponseDto>>> GetPatients([FromQuery] bool includeDeleted = false)
    {
        var patients = await _patientService.GetAllPatientsAsync(includeDeleted);
        return Ok(patients);
    }

    // GET: api/patients/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Doctor,Admin,Patient")] // Doctors/Admins can get any, Patient only their own
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientResponseDto>> GetPatient(int id, [FromQuery] bool includeDeleted = false)
    {
        var patient = await _patientService.GetPatientByIdAsync(id, includeDeleted);
        if (patient == null)
        {
            return NotFound($"Patient with ID {id} not found.");
        }

        // Policy-based authorization: Patient can only get their own record
        // Doctors and Admins are handled by the [Authorize(Roles="Doctor,Admin")]
        // So ResourceOwnerAuthorizationHandler will also succeed for Doctors/Admins if they try to access due to role based.
        if (User.IsInRole("Patient"))
        {
             var authorizationResult = await _authorizationService.AuthorizeAsync(User, patient, ResourceOperations.Read);
             if (!authorizationResult.Succeeded)
             {
                 return Forbid(); // Authenticated but not authorized to view other patients
             }
        }
        return Ok(patient);
    }

    // POST: api/patients
    [HttpPost]
    [AllowAnonymous] // Allows unauthenticated users to register
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
    [Authorize(Roles = "Patient,Admin")] // Patient can update their own, Admin can update any
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

        var existingPatient = await _patientService.GetPatientByIdAsync(id, includeDeleted: true); // Getting for auth check
        if (existingPatient == null)
        {
            return NotFound($"Patient with ID {id} not found.");
        }

        // Policy-based authorization: Patient can only update their own record
        if (User.IsInRole("Patient"))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, existingPatient, ResourceOperations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid(); //Trying to update other patient
            }
        }

        // Note: Admin role is covered by Role based so it will be able to update too.

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
    [Authorize(Roles = "Admin")]
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