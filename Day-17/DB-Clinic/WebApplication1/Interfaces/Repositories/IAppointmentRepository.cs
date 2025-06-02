public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync(bool includeDeleted = false);
    Task<Appointment?> GetAppointmentByNumberAsync(string appointmentNumber, bool includeDeleted = false);
    Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(int patientId, bool includeDeleted = false);
    Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(int doctorId, bool includeDeleted = false);
    Task<Appointment> AddAppointmentAsync(Appointment appointment);
    Task<Appointment?> RescheduleAppointmentAsync(string appointmentNumber, DateTime newTiming);
    Task<Appointment?> ChangeAppointmentStatusAsync(string appointmentNumber, string status);
    Task<bool> SoftDeleteAppointmentAsync(string appointmentNumber);
    Task<bool> AppointmentExistsAsync(string appointmentNumber, bool includeDeleted = false);
}