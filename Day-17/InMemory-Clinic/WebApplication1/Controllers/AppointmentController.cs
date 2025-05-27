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

    // GET: api/Appointments
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointments()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    // GET: api/Appointments/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentResponseDto>> GetAppointment(Guid id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return NotFound();
        }
        return Ok(appointment);
    }

    // GET: api/Appointments/Patient/{patientId}
    [HttpGet("Patient/{patientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointmentsByPatient(Guid patientId)
    {
        var appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);
        return Ok(appointments);
    }

    // GET: api/Appointments/Doctor/{doctorId}
    [HttpGet("Doctor/{doctorId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointmentsByDoctor(Guid doctorId)
    {
        var appointments = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId);
        return Ok(appointments);
    }

    // POST: api/Appointments
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AppointmentResponseDto>> PostAppointment([FromBody] AppointmentCreateDto appointmentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (addedAppointment, error) = await _appointmentService.AddAppointmentAsync(appointmentDto);
        if (addedAppointment == null)
        {
            return BadRequest(error);
        }
        return CreatedAtAction(nameof(GetAppointment), new { id = addedAppointment.Id }, addedAppointment);
    }

    // PUT: api/Appointments/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentResponseDto>> PutAppointment(Guid id, [FromBody] AppointmentUpdateDto appointmentDto)
    {
        if (id != appointmentDto.Id)
        {
            return BadRequest("ID in URL does not match ID in request body.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (updatedAppointment, error) = await _appointmentService.UpdateAppointmentAsync(appointmentDto);
        if (updatedAppointment == null)
        {
            if (error == "Appointment not found.")
            {
                return NotFound(error);
            }
            return BadRequest(error);
        }
        return Ok(updatedAppointment);
    }

    // PATCH: api/Appointments/{id}/status
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentResponseDto>> UpdateAppointmentStatus(Guid id, [FromQuery] AppointmentStatus newStatus)
    {
        var (updatedAppointment, error) = await _appointmentService.UpdateAppointmentStatusAsync(id, newStatus);
        if (updatedAppointment == null)
        {
            if (error == "Appointment not found.")
            {
                return NotFound(error);
            }
            return BadRequest(error);
        }
        return Ok(updatedAppointment);
    }


    // DELETE: api/Appointments/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAppointment(Guid id)
    {
        var appointmentExists = await _appointmentService.GetAppointmentByIdAsync(id) != null;
        if (!appointmentExists)
        {
            return NotFound();
        }

        await _appointmentService.DeleteAppointmentAsync(id);
        return NoContent();
    }
}