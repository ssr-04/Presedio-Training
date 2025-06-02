using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SpecialityRepository : ISpecialityRepository
{
    private readonly ClinicContext _context;

    public SpecialityRepository(ClinicContext context)
    {
        _context = context;
    }

    public async Task<Speciality> AddSpecialityAsync(Speciality speciality)
    {
        speciality.IsDeleted = false; // soft-delete flag is set false
        _context.Specialities.Add(speciality);
        await _context.SaveChangesAsync();
        return speciality;
    }

    public async Task<IEnumerable<Speciality>> GetAllSpecialitiesAsync(bool includeDeleted = false)
    {
        if (includeDeleted)
        {
            return await _context.Specialities
                                 .ToListAsync();
        }
        else
        {
            return await _context.Specialities
                                 .Where(s => !s.IsDeleted)
                                 .ToListAsync();
        }
    }

    public async Task<Speciality?> GetSpecialityByIdAsync(int id, bool includeDeleted = false)
    {
        if (includeDeleted)
        {
            return await _context.Specialities
                                 .FirstOrDefaultAsync(s => s.Id == id);
        }
        else
        {
            return await _context.Specialities
                                 .Where(s => !s.IsDeleted)
                                 .FirstOrDefaultAsync(s => s.Id == id);
        }
    }

    public async Task<bool> SpecialityExistsAsync(int id, bool includeDeleted = false)
    {
        if (includeDeleted)
        {
            return await _context.Specialities
                                 .AnyAsync(s => s.Id == id);
        }
        else
        {
            return await _context.Specialities
                                 .Where(s => !s.IsDeleted)
                                 .AnyAsync(s => s.Id == id);
        }
    }

    public async Task<bool> SpecialityExistsByNameAsync(string name, bool includeDeleted = false)
    {
        if (includeDeleted)
        {
            return await _context.Specialities
                                 .AnyAsync(s => s.Name == name);
        }
        else
        {
            return await _context.Specialities
                                 .Where(s => !s.IsDeleted)
                                 .AnyAsync(s => s.Name == name);
        }
    }

    public async Task<bool> SoftDeleteSpecialityAsync(int id)
    {
        var speciality = await _context.Specialities
                                     .FirstOrDefaultAsync(s => s.Id == id);

        if (speciality == null)
        {
            return false; // Speciality not found
        }

        if (speciality.IsDeleted)
        {
            return true; // Already soft-deleted
        }

        speciality.IsDeleted = true;
        _context.Specialities.Update(speciality);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Speciality?> UpdateSpecialityAsync(Speciality speciality)
    {
        var existingSpeciality = await _context.Specialities
                                                .Where(s => !s.IsDeleted) //exclude soft deleted
                                               .FirstOrDefaultAsync(s => s.Id == speciality.Id);

        if (existingSpeciality == null)
        {
            return null; // Speciality not found
        }

        // Update properties
        existingSpeciality.Name = speciality.Name;
        existingSpeciality.Status = speciality.Status;
        // Do NOT update IsDeleted here

        _context.Specialities.Update(existingSpeciality);
        await _context.SaveChangesAsync();
        return existingSpeciality;
    }
}