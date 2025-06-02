using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PatientRepository : IPatientRepository
{
    private readonly ClinicContext _context;

    public PatientRepository(ClinicContext context)
    {
        _context = context;
    }

    public async Task<Patient> AddPatientAsync(Patient patient)
    {
        // IsDeleted flag is correctly set for new patients as false
        patient.IsDeleted = false;

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task<Patient?> GetPatientByIdAsync(int id, bool includeDeleted = false)
    {
        if (includeDeleted)
        {
            return await _context.Patients
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }
        else
        {
            return await _context.Patients
                                 .Where(p => !p.IsDeleted)
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }
    }

    public async Task<IEnumerable<Patient>> GetAllPatientsAsync(bool includeDeleted = false)
    {
        if (includeDeleted)
        {
            return await _context.Patients
                                .ToListAsync();
        }
        else
        {
            return await _context.Patients
                                .Where(p => !p.IsDeleted)
                                .ToListAsync();
        }
    }

    public async Task<Patient?> UpdatePatientAsync(Patient patient)
    {
        var existingPatient = await _context.Patients
                                            .Where(p => !p.IsDeleted) // Check if it exists does not include soft delete status
                                            .FirstOrDefaultAsync(p => p.Id == patient.Id);

        if (existingPatient == null)
        {
            return null; // Patient not found
        }

        existingPatient.Name = patient.Name;
        existingPatient.Age = patient.Age;
        existingPatient.Email = patient.Email;
        existingPatient.Phone = patient.Phone;

        _context.Patients.Update(existingPatient); // Mark as modified
        await _context.SaveChangesAsync();
        return existingPatient; // Return the updated entity
    }

    public async Task<bool> SoftDeletePatientAsync(int id)
    {
        // Find the patient, potentially including soft-deleted ones to ensure we can "re-delete" if needed.
        var patient = await _context.Patients
                                    .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
        {
            return false; // Patient not found
        }

        if (patient.IsDeleted)
        {
            return true; // Already soft-deleted, consider it a success
        }

        patient.IsDeleted = true;
        _context.Patients.Update(patient); // Mark as modified
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PatientExistsAsync(int id, bool includeDeleted = false)
    {
        if (includeDeleted)
        {
            return await _context.Patients
                                 .AnyAsync(p => p.Id == id);
        }
        else
        {
            return await _context.Patients
                                 .Where(p => !p.IsDeleted)
                                 .AnyAsync(p => p.Id == id);
        }
    }

    public async Task<bool> PatientExistsByEmailAsync(string email, bool includeDeleted = false)
    {
        if (includeDeleted)
        {
            return await _context.Patients
                                 .AnyAsync(p => p.Email == email);
        }
        else
        {
            return await _context.Patients
                                 .Where(p => !p.IsDeleted)
                                 .AnyAsync(p => p.Email == email);
        }
    }
}