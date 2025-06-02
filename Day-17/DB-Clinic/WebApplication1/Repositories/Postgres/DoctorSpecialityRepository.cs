using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DoctorSpecialityRepository : IDoctorSpecialityRepository
{
    private readonly ClinicContext _context;

    public DoctorSpecialityRepository(ClinicContext context)
    {
        _context = context;
    }

    public async Task<DoctorSpeciality> AddDoctorSpecialityAsync(DoctorSpeciality doctorSpeciality)
    {
        _context.DoctorSpecialities.Add(doctorSpeciality);
        await _context.SaveChangesAsync();
        return doctorSpeciality;
    }

    public async Task<IEnumerable<DoctorSpeciality>> GetAllDoctorSpecialitiesAsync()
    {
        return await _context.DoctorSpecialities
                             .Include(ds => ds.Doctor)
                             .Include(ds => ds.Speciality)
                             .ToListAsync();
    }

    public async Task<DoctorSpeciality?> GetDoctorSpecialityBySerialNumberAsync(int serialNumber)
    {
        return await _context.DoctorSpecialities
                             .Include(ds => ds.Doctor)
                             .Include(ds => ds.Speciality)
                             .FirstOrDefaultAsync(ds => ds.SerialNumber == serialNumber);
    }

    public async Task<IEnumerable<DoctorSpeciality>> GetDoctorSpecialitiesByDoctorIdAsync(int doctorId)
    {
        return await _context.DoctorSpecialities
                             .Include(ds => ds.Doctor)
                             .Include(ds => ds.Speciality)
                             .Where(ds => ds.DoctorId == doctorId)
                             .ToListAsync();
    }

    public async Task<IEnumerable<DoctorSpeciality>> GetDoctorSpecialitiesBySpecialityIdAsync(int specialityId)
    {
        return await _context.DoctorSpecialities
                             .Include(ds => ds.Doctor)
                             .Include(ds => ds.Speciality)
                             .Where(ds => ds.SpecialityId == specialityId)
                             .ToListAsync();
    }

    public async Task<bool> RemoveDoctorSpecialityAsync(int serialNumber)
    {
        var doctorSpeciality = await _context.DoctorSpecialities.FindAsync(serialNumber);
        if (doctorSpeciality == null)
        {
            return false; // Not found
        }

        _context.DoctorSpecialities.Remove(doctorSpeciality);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DoctorSpecialityExistsAsync(int doctorId, int specialityId)
    {
        return await _context.DoctorSpecialities
                             .AnyAsync(ds => ds.DoctorId == doctorId && ds.SpecialityId == specialityId);
    }
}