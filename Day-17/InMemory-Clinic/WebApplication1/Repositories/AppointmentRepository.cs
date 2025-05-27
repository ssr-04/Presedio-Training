using System.Collections.Concurrent;

public class AppointmentRepository : IAppointmentRepository
{
    private static readonly ConcurrentDictionary<Guid, Appointment> _appointments = new();

    public AppointmentRepository()
    {
        // Seeding
        if (!_appointments.Any() && PatientRepository._patients.Any() && DoctorRepository._doctors.Any())
        {
            var patientId1 = PatientRepository._patients.Values.First().Id;
            var patientId2 = PatientRepository._patients.Values.Skip(1).First().Id;
            var doctorId1 = DoctorRepository._doctors.Values.First().Id;
            var doctorId2 = DoctorRepository._doctors.Values.Skip(1).First().Id;

            var appt1 = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = patientId1,
                DoctorId = doctorId1,
                AppointmentDateTime = DateTime.Now.AddDays(7).Date.AddHours(10),
                Reason = "Annual check-up",
                Status = AppointmentStatus.Scheduled
            };
            var appt2 = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = patientId2,
                DoctorId = doctorId2,
                AppointmentDateTime = DateTime.Now.AddDays(14).Date.AddHours(14),
                Reason = "Follow-up on blood test results",
                Status = AppointmentStatus.Scheduled
            };
            _appointments.TryAdd(appt1.Id, appt1);
            _appointments.TryAdd(appt2.Id, appt2);
        }
    }

    public Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return Task.FromResult(_appointments.Values.AsEnumerable());
    }

    public Task<Appointment?> GetAppointmentByIdAsync(Guid id)
    {
        _appointments.TryGetValue(id, out var appointment);
        return Task.FromResult(appointment);
    }

    public Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(Guid patientId)
    {
        return Task.FromResult(_appointments.Values.Where(a => a.PatientId == patientId).AsEnumerable());
    }

    public Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(Guid doctorId)
    {
        return Task.FromResult(_appointments.Values.Where(a => a.DoctorId == doctorId).AsEnumerable());
    }

    public Task<Appointment> AddAppointmentAsync(Appointment appointment)
    {
        appointment.Id = Guid.NewGuid(); // creating a new ID
        _appointments.TryAdd(appointment.Id, appointment);
        return Task.FromResult(appointment);
    }

    public Task<Appointment?> UpdateAppointmentAsync(Appointment appointment)
    {
        if (_appointments.ContainsKey(appointment.Id))
        {
            _appointments[appointment.Id] = appointment;
            return Task.FromResult<Appointment?>(appointment);
        }
        return Task.FromResult<Appointment?>(null);
    }

    public Task<bool> DeleteAppointmentAsync(Guid id)
    {
        return Task.FromResult(_appointments.TryRemove(id, out _));
    }

    public Task<bool> AppointmentExistsAsync(Guid id)
    {
        return Task.FromResult(_appointments.ContainsKey(id));
    }
}