public interface IAppointmentService
{
    Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync();
    Task<AppointmentResponseDto?> GetAppointmentByIdAsync(Guid id);
    Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByPatientIdAsync(Guid patientId);
    Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByDoctorIdAsync(Guid doctorId);
    Task<(AppointmentResponseDto? appointment, string? error)> AddAppointmentAsync(AppointmentCreateDto appointmentDto);
    Task<(AppointmentResponseDto? appointment, string? error)> UpdateAppointmentStatusAsync(Guid appointmentId, AppointmentStatus newStatus);
    Task<(AppointmentResponseDto? appointment, string? error)> UpdateAppointmentAsync(AppointmentUpdateDto appointmentDto);
    Task<bool> DeleteAppointmentAsync(Guid id);
}