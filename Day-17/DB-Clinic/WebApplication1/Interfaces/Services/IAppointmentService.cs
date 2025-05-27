public interface IAppointmentService
{
    Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync();
    Task<AppointmentResponseDto?> GetAppointmentByNumberAsync(string appointmentNumber);
    Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByPatientIdAsync(int patientId);
    Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByDoctorIdAsync(int doctorId);
    Task<(AppointmentResponseDto? appointment, string? error)> AddAppointmentAsync(AppointmentCreateDto appointmentDto);
    Task<(AppointmentResponseDto? appointment, string? error)> UpdateAppointmentAsync(AppointmentUpdateDto appointmentDto);
    Task<bool> DeleteAppointmentAsync(string appointmentNumber); // Soft delete
}