using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DoctorRepository : IDoctorRepository
{
    private readonly ClinicContext _context;

    public DoctorRepository(ClinicContext context)
    {
        _context = context;
    }

    public async Task<Doctor> AddDoctorAsync(Doctor doctor)
    {
        doctor.IsDeleted = false; // soft-delete flag is set to false
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();
        return doctor;
    }

    public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync(bool includeDeleted = false)
    {
        IQueryable<Doctor> query = _context.Doctors
                                            .Include(d => d.DoctorSpecialities!)
                                            .ThenInclude(ds => ds.Speciality); 
        if (!includeDeleted)
        {
            query = query.Where(d => !d.IsDeleted);

        }
        return await query.ToListAsync();

    }

    public async Task<Doctor?> GetDoctorByIdAsync(int id, bool includeDeleted = false)
    {
        IQueryable<Doctor> query = _context.Doctors
                                    .Include(d => d.DoctorSpecialities!) 
                                    .ThenInclude(ds => ds.Speciality);

        if (!includeDeleted)
        {
            query = query.Where(d => !d.IsDeleted);
        }
        return await query.FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<bool> DoctorExistsAsync(int id, bool includeDeleted = false)
    {
        if (includeDeleted)
        {
            return await _context.Doctors
                                 .AnyAsync(d => d.Id == id);
        }
        else
        {
            return await _context.Doctors
                                 .Where(d => !d.IsDeleted) 
                                 .AnyAsync(d => d.Id == id);
        }
    }

    public async Task<bool> DoctorExistsByEmailAsync(string email, bool includeDeleted = false)
    {
        if (includeDeleted)
        {
            return await _context.Doctors
                                 .AnyAsync(d => d.Email == email);
        }
        else
        {
            return await _context.Doctors
                                 .Where(p => !p.IsDeleted)
                                 .AnyAsync(d => d.Email == email);
        }
    }

    public async Task<bool> SoftDeleteDoctorAsync(int id)
    {
        var doctor = await _context.Doctors
                                   .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor == null)
        {
            return false; // Doctor not found
        }

        if (doctor.IsDeleted)
        {
            return true; // Already soft-deleted
        }

        var hasFutureAppointments = await _context.Appointments
                                                  .Where(a => a.DoctorId == id && a.AppointmentDateTime > DateTime.Now && !a.IsDeleted && a.Status != "Cancelled" && a.Status != "Completed")
                                                  .AnyAsync();
        if (hasFutureAppointments)
        {
            throw new InvalidOperationException($"Cannot soft delete Doctor ID {id} as they have active future appointments.");
        }

        doctor.IsDeleted = true;
        _context.Doctors.Update(doctor);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Doctor?> UpdateDoctorAsync(Doctor doctor)
    {
        var existingDoctor = await _context.Doctors
                                           .Where(p => !p.IsDeleted) //exclude soft deleted
                                           .FirstOrDefaultAsync(d => d.Id == doctor.Id);

        if (existingDoctor == null)
        {
            return null; // Doctor not found
        }

        // Update properties
        existingDoctor.Name = doctor.Name;
        existingDoctor.Status = doctor.Status;
        existingDoctor.YearsOfExperience = doctor.YearsOfExperience;
        existingDoctor.Email = doctor.Email;
        existingDoctor.Phone = doctor.Phone;

        _context.Doctors.Update(existingDoctor);
        await _context.SaveChangesAsync();
        return existingDoctor;
    }
}