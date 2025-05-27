public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<Appointment?> GetAppointmentByIdAsync(Guid id);
    Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(Guid patientId);
    Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(Guid doctorId);
    Task<Appointment> AddAppointmentAsync(Appointment appointment);
    Task<Appointment?> UpdateAppointmentAsync(Appointment appointment);
    Task<bool> DeleteAppointmentAsync(Guid id);
    Task<bool> AppointmentExistsAsync(Guid id);
}