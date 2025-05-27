public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository; // To validate PatientId and get patient name
    private readonly IDoctorRepository _doctorRepository;   // same as for patient

    public AppointmentService(IAppointmentRepository appointmentRepository,
                                IPatientRepository patientRepository,
                                IDoctorRepository doctorRepository)
    {
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
    }

    private async Task<AppointmentResponseDto> MapToAppointmentResponseDto(Appointment appointment)
    {
        var patient = await _patientRepository.GetPatientByIdAsync(appointment.PatientId);
        var doctor = await _doctorRepository.GetDoctorByIdAsync(appointment.DoctorId);

        return new AppointmentResponseDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentDateTime = appointment.AppointmentDateTime,
            Reason = appointment.Reason,
            Status = appointment.Status,
            //Just having if check so we don't get warning, as GetById maybe nullable by defintion but in practice not null for sure
            PatientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "Unknown Patient", 
            DoctorName = doctor != null ? $"{doctor.FirstName} {doctor.LastName}" : "Unknown Doctor"
        };
    }

    public async Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync()
    {
        var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
        var responseDtos = new List<AppointmentResponseDto>();
        foreach (var appt in appointments)
        {
            responseDtos.Add(await MapToAppointmentResponseDto(appt));
        }
        return responseDtos;
    }

    public async Task<AppointmentResponseDto?> GetAppointmentByIdAsync(Guid id)
    {
        var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
        if (appointment == null) return null;
        return await MapToAppointmentResponseDto(appointment);
    }

    public async Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByPatientIdAsync(Guid patientId)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByPatientIdAsync(patientId);
        var responseDtos = new List<AppointmentResponseDto>();
        foreach (var appt in appointments)
        {
            responseDtos.Add(await MapToAppointmentResponseDto(appt));
        }
        return responseDtos;
    }

    public async Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByDoctorIdAsync(Guid doctorId)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByDoctorIdAsync(doctorId);
        var responseDtos = new List<AppointmentResponseDto>();
        foreach (var appt in appointments)
        {
            responseDtos.Add(await MapToAppointmentResponseDto(appt));
        }
        return responseDtos;
    }

    public async Task<(AppointmentResponseDto? appointment, string? error)> AddAppointmentAsync(AppointmentCreateDto appointmentDto)
    {
        // validation
        if (!await _patientRepository.PatientExistsAsync(appointmentDto.PatientId))
        {
            return (null, "Patient not found for the given Patient ID.");
        }
        if (!await _doctorRepository.DoctorExistsAsync(appointmentDto.DoctorId))
        {
            return (null, "Doctor not found for the given Doctor ID.");
        }
        if (appointmentDto.AppointmentDateTime <= DateTime.Now)
        {
            return (null, "Appointment date and time must be in the future.");
        }

        var appointment = new Appointment
        {
            PatientId = appointmentDto.PatientId,
            DoctorId = appointmentDto.DoctorId,
            AppointmentDateTime = appointmentDto.AppointmentDateTime,
            Reason = appointmentDto.Reason,
            Status = appointmentDto.Status // DTO can specify, or will use default as scheduled
        };

        var addedAppointment = await _appointmentRepository.AddAppointmentAsync(appointment);
        return (await MapToAppointmentResponseDto(addedAppointment), null);
    }

    public async Task<(AppointmentResponseDto? appointment, string? error)> UpdateAppointmentAsync(AppointmentUpdateDto appointmentDto)
    {
        // validation
        var existingAppointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentDto.Id);
        if (existingAppointment == null)
        {
            return (null, "Appointment not found.");
        }
        if (!await _patientRepository.PatientExistsAsync(appointmentDto.PatientId))
        {
            return (null, "Patient not found for the given Patient ID.");
        }
        if (!await _doctorRepository.DoctorExistsAsync(appointmentDto.DoctorId))
        {
            return (null, "Doctor not found for the given Doctor ID.");
        }
        if (appointmentDto.AppointmentDateTime <= DateTime.Now)
        {
            return (null, "Appointment date and time must be in the future.");
        }

        existingAppointment.PatientId = appointmentDto.PatientId;
        existingAppointment.DoctorId = appointmentDto.DoctorId;
        existingAppointment.AppointmentDateTime = appointmentDto.AppointmentDateTime;
        existingAppointment.Reason = appointmentDto.Reason;
        existingAppointment.Status = appointmentDto.Status;

        var updatedAppointment = await _appointmentRepository.UpdateAppointmentAsync(existingAppointment);
        if (updatedAppointment == null) return (null, "Failed to update appointment."); // Should not happen if exists check passed

        return (await MapToAppointmentResponseDto(updatedAppointment), null);
    }

    public async Task<(AppointmentResponseDto? appointment, string? error)> UpdateAppointmentStatusAsync(Guid appointmentId, AppointmentStatus newStatus)
    {
        var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
        if (appointment == null)
        {
            return (null, "Appointment not found.");
        }

        // some validation
        if (appointment.Status == AppointmentStatus.Completed && newStatus == AppointmentStatus.Scheduled)
        {
            return (null, "Cannot reschedule a completed appointment.");
        }
        // Can do for cancelled appointment too
        appointment.Status = newStatus;
        var updatedAppointment = await _appointmentRepository.UpdateAppointmentAsync(appointment);
        if (updatedAppointment == null) return (null, "Failed to update appointment status.");

        return (await MapToAppointmentResponseDto(updatedAppointment), null);
    }

    public async Task<bool> DeleteAppointmentAsync(Guid id)
    {
        return await _appointmentRepository.DeleteAppointmentAsync(id);
    }
}