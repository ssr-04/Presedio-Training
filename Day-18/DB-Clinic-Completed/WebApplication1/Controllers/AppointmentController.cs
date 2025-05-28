using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    // GET: api/appointments
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointments([FromQuery] bool includeDeleted = false)
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync(includeDeleted);
        return Ok(appointments);
    }

    // GET: api/appointments/{appointmentNumber}
    [HttpGet("{appointmentNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentResponseDto>> GetAppointment(string appointmentNumber, [FromQuery] bool includeDeleted = false)
    {
        var appointment = await _appointmentService.GetAppointmentByNumberAsync(appointmentNumber, includeDeleted);
        if (appointment == null)
        {
            return NotFound($"Appointment with number '{appointmentNumber}' not found.");
        }
        return Ok(appointment);
    }

    // GET: api/appointments/byPatient/{patientId}
    [HttpGet("byPatient/{patientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointmentsByPatientId(int patientId, [FromQuery] bool includeDeleted = false)
    {
        var appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId, includeDeleted);
        return Ok(appointments);
    }

    // GET: api/appointments/byDoctor/{doctorId}
    [HttpGet("byDoctor/{doctorId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointmentsByDoctorId(int doctorId, [FromQuery] bool includeDeleted = false)
    {
        var appointments = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId, includeDeleted);
        return Ok(appointments);
    }

    // POST: api/appointments
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AppointmentResponseDto>> PostAppointment([FromBody] AppointmentCreateDto appointmentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
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

    // DELETE: api/appointments/{appointmentNumber} (Soft Delete)
    [HttpDelete("{appointmentNumber}")]
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