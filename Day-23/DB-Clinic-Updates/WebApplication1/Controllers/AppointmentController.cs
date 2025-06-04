using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService; // Needed for policy check
    private readonly IDoctorService _doctorService; // Needed for policy check
    private readonly IAuthorizationService _authorizationService;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IPatientService patientService,
        IDoctorService doctorService,
        IAuthorizationService authorizationService)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
        _doctorService = doctorService;
        _authorizationService = authorizationService;
    }

    // GET: api/appointments
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")] //Admin any, doctors/patients only theirs
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointments([FromQuery] bool includeDeleted = false)
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync(includeDeleted);
        if (User.IsInRole("Doctor") || User.IsInRole("Patient"))
        {
            var emailClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (emailClaim == null)
            {
                return Forbid();
            }
            var email = emailClaim.Value;
            if (User.IsInRole("Doctor"))
            {
                var doctor = (await _doctorService.GetAllDoctorsAsync()).FirstOrDefault(d => d.Email == email);
                if (doctor == null)
                {
                    return Forbid();
                }
                appointments = appointments.Where(a => a.Doctor != null && a.Doctor.Id == doctor.Id);
            }
            else if (User.IsInRole("Patient"))
            {
                var patient = (await _patientService.GetAllPatientsAsync()).FirstOrDefault(p => p.Email == email);
                if (patient == null)
                {
                    return Forbid();
                }
                appointments = appointments.Where(a => a.Patient != null && a.Patient.Id == patient.Id);
            }
        }
        return Ok(appointments);
    }

    // GET: api/appointments/{appointmentNumber}
    [HttpGet("{appointmentNumber}")]
    [Authorize(Roles = "Doctor,Patient,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentResponseDto>> GetAppointment(string appointmentNumber, [FromQuery] bool includeDeleted = false)
    {
        var appointment = await _appointmentService.GetAppointmentByNumberAsync(appointmentNumber, includeDeleted);
        if (appointment == null)
        {
            return NotFound($"Appointment with number '{appointmentNumber}' not found.");
        }

        // Policy-based authorization: Patient/Doctor can only view their own associated appointments
        if (User.IsInRole("Patient") || User.IsInRole("Doctor"))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, appointment, ResourceOperations.Read);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }
        // Admin can view any as it's handled by Role based

        return Ok(appointment);
    }

    // GET: api/appointments/byPatient/{patientId}
    [HttpGet("byPatient/{patientId}")]
    [Authorize(Roles = "Patient,Admin")] // Patient can get only theirs while Admins can get any
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointmentsByPatientId(int patientId, [FromQuery] bool includeDeleted = false)
    {
        // Policy-based authorization: Patient can only get appointments for their own ID
        if (User.IsInRole("Patient"))
        {
            var patient = await _patientService.GetPatientByIdAsync(patientId); // Get resource for auth check
            if (patient == null) return NotFound($"Patient with ID {patientId} not found.");

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, patient, ResourceOperations.Read); // Check ownership of patient
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }
        var appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId, includeDeleted);
        return Ok(appointments);
    }

    // GET: api/appointments/byDoctor/{doctorId}
    [HttpGet("byDoctor/{doctorId}")]
    [Authorize(Roles = "Doctor,Admin")] // Doctors only thiers while admin any
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointmentsByDoctorId(int doctorId, [FromQuery] bool includeDeleted = false)
    {
        // Policy-based authorization: Doctor can only get appointments for their own ID
        if (User.IsInRole("Doctor"))
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(doctorId); // Get resource for auth check
            if (doctor == null) return NotFound($"Doctor with ID {doctorId} not found.");

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, doctor, ResourceOperations.Read); // Check ownership of doctor
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }
        var appointments = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId, includeDeleted);
        return Ok(appointments);
    }

    // POST: api/appointments
    [HttpPost]
    [Authorize(Roles = "Patient,Admin")] // Patient can only book for themselves, admin can book for any
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AppointmentResponseDto>> PostAppointment([FromBody] AppointmentCreateDto appointmentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Policy-based authorization: Patient can only book for themselves
        if (User.IsInRole("Patient"))
        {
            // We need to get the patient entity associated using the DTO's patientId
            // for the ResourceOwnerAuthorizationHandler check.
            var targetPatient = await _patientService.GetPatientByIdAsync(appointmentDto.PatientId);
            if (targetPatient == null)
            {
                return NotFound($"Patient with ID {appointmentDto.PatientId} not found.");
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, appointmentDto, ResourceOperations.Create);
            if (!authorizationResult.Succeeded)
            {
                return Forbid(); // Patient trying to book for someone else
            }
        }

        try
        {
            var createdAppointment = await _appointmentService.AddAppointmentAsync(appointmentDto);
            return CreatedAtAction(nameof(GetAppointment), new { appointmentNumber = createdAppointment.AppointmentNumber }, createdAppointment);
        }
        catch (ArgumentException ex) // For invalid Patient/Doctor IDs
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex) // For future timing
        {
            return BadRequest(ex.Message); // Return 400 Bad Request
        }
    }

    // PUT: api/appointments/{appointmentNumber}/reschedule
    [HttpPut("{appointmentNumber}/reschedule")]
    [Authorize(Roles = "Doctor,Patient,Admin")] //Patient/Doctor can only update for their own
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentResponseDto>> RescheduleAppointment(
        string appointmentNumber,
        [FromBody] AppointmentRescheduleDto rescheduleDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingAppointment = await _appointmentService.GetAppointmentByNumberAsync(appointmentNumber, includeDeleted: true);
        if (existingAppointment == null)
        {
            return NotFound($"Appointment with number '{appointmentNumber}' not found.");
        }

        // Policy-based authorization: Patient/Doctor can only reschedule their own appointments
        if (User.IsInRole("Patient") || User.IsInRole("Doctor"))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, existingAppointment, ResourceOperations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        try
        {
            var updatedAppointment = await _appointmentService.RescheduleAppointmentAsync(appointmentNumber, rescheduleDto);
            if (updatedAppointment == null)
            {
                return NotFound($"Appointment with number '{appointmentNumber}' not found.");
            }
            return Ok(updatedAppointment);
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message); //  cancelled/completed, or past timing
        }
    }

    // PUT: api/appointments/{appointmentNumber}/status
    [HttpPut("{appointmentNumber}/status")]
    [Authorize(Roles = "Doctor,Admin")] // Only Doctors (or Admin) can change status.
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentResponseDto>> ChangeAppointmentStatus(
        string appointmentNumber,
        [FromBody] AppointmentStatusChangeDto statusChangeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get the appointment first to check existence
        var existingAppointment = await _appointmentService.GetAppointmentByNumberAsync(appointmentNumber, includeDeleted: true);
        if (existingAppointment == null)
        {
            return NotFound($"Appointment with number '{appointmentNumber}' not found.");
        }

        try
        {
            var updatedAppointment = await _appointmentService.ChangeAppointmentStatusAsync(appointmentNumber, statusChangeDto);
            if (updatedAppointment == null)
            {
                return NotFound($"Appointment with number '{appointmentNumber}' not found.");
            }
            return Ok(updatedAppointment);
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message); // invalid status transitions
        }
    }

    // PUT: api/appointments/{appointmentNumber} (Soft Delete)
    [HttpPut("cancel/{appointmentNumber}")]
    [Authorize(Policy = "DoctorHasMinimumExperienceToCancel", Roles = "Admin,Doctor,Patient")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelAppointment(string appointmentNumber)
    {
        // Get the appointment first to check existence
        var existingAppointment = await _appointmentService.GetAppointmentByNumberAsync(appointmentNumber, includeDeleted: true);
        if (existingAppointment == null)
        {
            return NotFound($"Appointment with number '{appointmentNumber}' not found.");
        }

        // Policy-based authorization: Patient/Doctor can only cancel their own appointments
        if (User.IsInRole("Patient") || User.IsInRole("Doctor"))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, existingAppointment, ResourceOperations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        try
        {
            var CancelledAppointment = await _appointmentService.CancelAppointment(appointmentNumber);
            if (CancelledAppointment == null)
            {
                return NotFound($"Appointment with number '{appointmentNumber}' not found.");
            }
            return Ok(CancelledAppointment);
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message); //  alreay cancelled/completed
        }
    }

    // DELETE: api/appointments/{appointmentNumber} (Soft Delete)
    [HttpDelete("{appointmentNumber}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAppointment(string appointmentNumber)
    {
        try
        {
            var deleted = await _appointmentService.SoftDeleteAppointmentAsync(appointmentNumber);
            if (!deleted)
            {
                return NotFound($"Appointment with number '{appointmentNumber}' not found or already deleted.");
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message); // like cannot delete completed appointments
        }
    }
}