public interface IAppointmentService
{
    Task<AppointmentResponseDto?> GetAppointmentByNumberAsync(string appointmentNumber, bool includeDeleted = false);
    Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync(bool includeDeleted = false);
    Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByPatientIdAsync(int patientId, bool includeDeleted = false);
    Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByDoctorIdAsync(int doctorId, bool includeDeleted = false);
    Task<AppointmentResponseDto> AddAppointmentAsync(AppointmentCreateDto appointmentDto);
    Task<bool> SoftDeleteAppointmentAsync(string appointmentNumber);
    Task<AppointmentResponseDto?> RescheduleAppointmentAsync(string appointmentNumber, AppointmentRescheduleDto rescheduleDto);
    Task<AppointmentResponseDto?> ChangeAppointmentStatusAsync(string appointmentNumber, AppointmentStatusChangeDto statusChangeDto);
    Task<AppointmentResponseDto?> CancelAppointment(string appointmentNumber);
    Task<bool> AppointmentExistsAsync(string appointmentNumber, bool includeDeleted = false);
}
