using System.Collections.Concurrent;

public class PatientRepository : IPatientRepository
{
    // In-memory storage 
    public static readonly ConcurrentDictionary<Guid, Patient> _patients = new();

    /* Note: As we are using in-memory repo and all the operations are done synchorously,
     but we defined output as Task to make it future proof for future async db operations,
     The use of Task.FromResult allows to satisfy the async contract without 
     actually running code asynchronously.
    */

    public PatientRepository()
    {
        // Seeding
        if (!_patients.Any())
        {
            var patient1 = new Patient
            {
                Id = Guid.NewGuid(),
                FirstName = "Ramu",
                LastName = "R",
                DateOfBirth = new DateTime(2000, 5, 10),
                Email = "ramu@gmail.com",
                PhoneNumber = "9876543210"
            };
            var patient2 = new Patient
            {
                Id = Guid.NewGuid(),
                FirstName = "Somu",
                LastName = "S",
                DateOfBirth = new DateTime(1990, 8, 20),
                Email = "Somu@gmail.com",
                PhoneNumber = "8976543210"
            };
            _patients.TryAdd(patient1.Id, patient1);
            _patients.TryAdd(patient2.Id, patient2);
        }
    }

    public Task<IEnumerable<Patient>> GetAllPatientsAsync()
    {
        return Task.FromResult(_patients.Values.AsEnumerable());
    }

    public Task<Patient?> GetPatientByIdAsync(Guid id)
    {
        _patients.TryGetValue(id, out var patient);
        return Task.FromResult(patient);
    }

    public Task<Patient> AddPatientAsync(Patient patient)
    {
        patient.Id = Guid.NewGuid(); // creates a new ID
        _patients.TryAdd(patient.Id, patient);
        return Task.FromResult(patient);
    }

    public Task<Patient?> UpdatePatientAsync(Patient patient)
    {
        if (_patients.ContainsKey(patient.Id))
        {
            _patients[patient.Id] = patient; 
            return Task.FromResult<Patient?>(patient);
        }
        return Task.FromResult<Patient?>(null);
    }

    public Task<bool> DeletePatientAsync(Guid id)
    {
        return Task.FromResult(_patients.TryRemove(id, out _));
        //Second param is used to store object removed, _ after out means discard it
    }

    public Task<bool> PatientExistsAsync(Guid id)
    {
        return Task.FromResult(_patients.ContainsKey(id));
    }

    public Task<bool> PatientExistsByEmailAsync(string email)
    {
        return Task.FromResult(_patients.Values.Any(p => p.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
    }
}