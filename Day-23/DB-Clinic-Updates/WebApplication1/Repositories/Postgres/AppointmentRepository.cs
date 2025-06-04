using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ClinicContext _context;

    public AppointmentRepository(ClinicContext context)
    {
        _context = context;
    }

    public async Task<Appointment> AddAppointmentAsync(Appointment appointment)
    {
        appointment.IsDeleted = false;
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return await GetAppointmentByNumberAsync(appointment.AppointmentNumber);
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync(bool includeDeleted = false)
    {
        IQueryable<Appointment> query = _context.Appointments
                                                .Include(a => a.Patient)
                                                .Include(a => a.Doctor);

        if (!includeDeleted)
        {
            query = query.Where(a => !a.IsDeleted);
        }

        return await query.ToListAsync();
    }

    public async Task<Appointment?> GetAppointmentByNumberAsync(string appointmentNumber, bool includeDeleted = false)
    {
        IQueryable<Appointment> query = _context.Appointments
                                                .Include(a => a.Patient)
                                                .Include(a => a.Doctor);

        if (!includeDeleted)
        {
            query = query.Where(a => !a.IsDeleted);
        }

        return await query.FirstOrDefaultAsync(a => a.AppointmentNumber == appointmentNumber);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(int patientId, bool includeDeleted = false)
    {
        IQueryable<Appointment> query = _context.Appointments
                                                .Include(a => a.Patient)
                                                .Include(a => a.Doctor);

        if (!includeDeleted)
        {
            query = query.Where(a => !a.IsDeleted);
        }

        return await query.Where(a => a.PatientId == patientId).ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(int doctorId, bool includeDeleted = false)
    {
        IQueryable<Appointment> query = _context.Appointments
                                                .Include(a => a.Patient)
                                                .Include(a => a.Doctor);

        if (!includeDeleted)
        {
            query = query.Where(a => !a.IsDeleted);
        }

        return await query.Where(a => a.DoctorId == doctorId).ToListAsync();
    }

    public async Task<bool> AppointmentExistsAsync(string appointmentNumber, bool includeDeleted = false)
    {
        if (includeDeleted)
        {
            return await _context.Appointments
                                 .AnyAsync(a => a.AppointmentNumber == appointmentNumber);
        }
        else
        {
            return await _context.Appointments
                                 .Where(a => !a.IsDeleted)
                                 .AnyAsync(a => a.AppointmentNumber == appointmentNumber);
        }
    }

    public async Task<bool> SoftDeleteAppointmentAsync(string appointmentNumber)
    {
        var appointment = await _context.Appointments
                                        .FirstOrDefaultAsync(a => a.AppointmentNumber == appointmentNumber);

        if (appointment == null)
        {
            return false; // Appointment not found
        }

        if (appointment.IsDeleted)
        {
            return true; // Already soft-deleted
        }

        appointment.IsDeleted = true;
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Appointment?> RescheduleAppointmentAsync(string appointmentNumber, DateTime newTiming)
    {
        var existingAppointment = await GetAppointmentByNumberAsync(appointmentNumber); //doesn't have soft deleted ones

        if (existingAppointment == null)
        {
            return null; // Appointment not found
        }

        // Update properties
        // some logic like can't reschedule cancelled or completed appointments/past
        existingAppointment.AppointmentDateTime = newTiming;

        _context.Appointments.Update(existingAppointment);
        await _context.SaveChangesAsync();
        return existingAppointment;
    }

    public async Task<Appointment?> ChangeAppointmentStatusAsync(string appointmentNumber, string status)
    {
        // Fetch the appointment, ignoring soft delete filter
        var appointment = await GetAppointmentByNumberAsync(appointmentNumber); //doesn't have soft deleted ones

        if (appointment == null)
        {
            return null; // Appointment not found
        }

        // Need to add logic for status transitions here, like can't change from cancelled to schduled or confirmed to scheduled

        appointment.Status = status;

        _context.Appointments.Update(appointment); 
        await _context.SaveChangesAsync();
        return appointment;
    }

}