using System.Collections.Concurrent;

public class DoctorRepository : IDoctorRepository
    {
        public static readonly ConcurrentDictionary<Guid, Doctor> _doctors = new();

        public DoctorRepository()
        {
            // Seeding
            if (!_doctors.Any())
            {
                var doctor1 = new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Shaun",
                    LastName = "Murphy",
                    Specialty = "NeuroSurgery",
                    Email = "shuan@gmail.com",
                    PhoneNumber = "1234567890"
                };
                var doctor2 = new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Alan",
                    LastName = "Glassman",
                    Specialty = "Pediatrics",
                    Email = "glassman@gmail.com",
                    PhoneNumber = "2345678910"
                };
                _doctors.TryAdd(doctor1.Id, doctor1);
                _doctors.TryAdd(doctor2.Id, doctor2);
            }
        }

        public Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
        {
            return Task.FromResult(_doctors.Values.AsEnumerable());
        }

        public Task<Doctor?> GetDoctorByIdAsync(Guid id)
        {
            _doctors.TryGetValue(id, out var doctor);
            return Task.FromResult(doctor);
        }

        public Task<Doctor> AddDoctorAsync(Doctor doctor)
        {
            doctor.Id = Guid.NewGuid();
            _doctors.TryAdd(doctor.Id, doctor);
            return Task.FromResult(doctor);
        }

        public Task<Doctor?> UpdateDoctorAsync(Doctor doctor)
        {
            if (_doctors.ContainsKey(doctor.Id))
            {
                _doctors[doctor.Id] = doctor;
                return Task.FromResult<Doctor?>(doctor);
            }
            return Task.FromResult<Doctor?>(null);
        }

        public Task<bool> DeleteDoctorAsync(Guid id)
        {
            return Task.FromResult(_doctors.TryRemove(id, out _));
        }

        public Task<bool> DoctorExistsAsync(Guid id)
        {
            return Task.FromResult(_doctors.ContainsKey(id));
        }

        public Task<bool> DoctorExistsByEmailAsync(string email)
        {
            return Task.FromResult(_doctors.Values.Any(d => d.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        }
    }