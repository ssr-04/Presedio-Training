using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicApi.Tests.Repositories
{
    [TestFixture]
    public class DoctorRepositoryTests
    {
        private ClinicContext _context;
        private DoctorRepository _doctorRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ClinicContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
            _context = new ClinicContext(options);
            _context.Database.EnsureCreated();
            _doctorRepository = new DoctorRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddDoctorAsync()
        {
            var newDoctor = new Doctor
            {
                Name = "Shaun",
                Status = "Active",
                YearsOfExperience = 5,
                Email = "shaun@gmail.com",
                Phone = "1234567890"
            };

            var addedDoctor = await _doctorRepository.AddDoctorAsync(newDoctor);

            Assert.That(addedDoctor, Is.Not.Null);
            Assert.That(addedDoctor.Id, Is.GreaterThan(0));
            Assert.That(addedDoctor.Name, Is.EqualTo(newDoctor.Name));
            Assert.That(addedDoctor.Email, Is.EqualTo(newDoctor.Email));
            Assert.That(addedDoctor.IsDeleted, Is.False);

            var doctorInDb = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == addedDoctor.Id);
            Assert.That(doctorInDb, Is.Not.Null);
            Assert.That(doctorInDb.Name, Is.EqualTo(addedDoctor.Name));
            Assert.That(doctorInDb.Email, Is.EqualTo(addedDoctor.Email));
            Assert.That(doctorInDb.IsDeleted, Is.False);
        }

        [Test]
        public async Task GetDoctorByIdAsync_ShouldReturnDoctor_WhenDoctorExists()
        {
            var newDoctor = new Doctor
            {
                Name = "Shaun",
                Status = "Active",
                YearsOfExperience = 5,
                Email = "shaun@gmail.com",
                Phone = "1234567890"
            };
            var addedDoctor = await _doctorRepository.AddDoctorAsync(newDoctor);

            var doctor = await _doctorRepository.GetDoctorByIdAsync(addedDoctor.Id);

            Assert.That(doctor, Is.Not.Null);
            Assert.That(doctor.Id, Is.EqualTo(addedDoctor.Id));
            Assert.That(doctor.Name, Is.EqualTo(newDoctor.Name));
        }

        [Test]
        public async Task GetDoctorByIdAsync_ShouldReturnNull_WhenDoctorDoesNotExist()
        {
            var doctor = await _doctorRepository.GetDoctorByIdAsync(999);

            Assert.That(doctor, Is.Null);
        }

        [Test]
        public async Task GetDoctorByIdAsync_ShouldReturnDeletedDoctor_WhenIncludeDeletedIsTrue()
        {
            var deletedDoctor = new Doctor { Name = "Deleted Doc", Email = "deleted@example.com", IsDeleted = true };
            _context.Doctors.Add(deletedDoctor);
            await _context.SaveChangesAsync();

            var doctor = await _doctorRepository.GetDoctorByIdAsync(deletedDoctor.Id, includeDeleted: true);

            Assert.That(doctor, Is.Not.Null);
            Assert.That(doctor.IsDeleted, Is.True);
        }

        [Test]
        public async Task GetAllDoctorsAsync_ShouldReturnAllActiveDoctors()
        {
            await _doctorRepository.AddDoctorAsync(new Doctor { Name = "Doc1", Email = "doc1@example.com" });
            await _doctorRepository.AddDoctorAsync(new Doctor { Name = "Doc2", Email = "doc2@example.com" });
            _context.Doctors.Add(new Doctor { Name = "Doc3", Email = "doc3@example.com", IsDeleted = true });
            await _context.SaveChangesAsync();

            var doctors = await _doctorRepository.GetAllDoctorsAsync();

            Assert.That(doctors.Count(), Is.EqualTo(2));
            Assert.That(doctors.Any(d => d.Name == "Doc1"), Is.True);
            Assert.That(doctors.Any(d => d.Name == "Doc2"), Is.True);
            Assert.That(doctors.Any(d => d.Name == "Doc3"), Is.False);
        }

        [Test]
        public async Task GetAllDoctorsAsync_ShouldReturnAllDoctors_WhenIncludeDeletedIsTrue()
        {
            await _doctorRepository.AddDoctorAsync(new Doctor { Name = "Doc1", Email = "doc1@example.com" });
            _context.Doctors.Add(new Doctor { Name = "Doc2", Email = "doc2@example.com", IsDeleted = true });
            await _context.SaveChangesAsync();

            var doctors = await _doctorRepository.GetAllDoctorsAsync(includeDeleted: true);

            Assert.That(doctors.Count(), Is.EqualTo(2));
            Assert.That(doctors.Any(d => d.Name == "Doc1"), Is.True);
            Assert.That(doctors.Any(d => d.Name == "Doc2"), Is.True);
        }

        [Test]
        public async Task UpdateDoctorAsync_ShouldUpdateDoctorSuccessfully()
        {
            var doctor = new Doctor { Name = "Original", Email = "original@example.com", Status = "Active", YearsOfExperience = 5, Phone = "123" };
            await _doctorRepository.AddDoctorAsync(doctor);

            doctor.Name = "Updated";
            doctor.YearsOfExperience = 6;

            var updatedDoctor = await _doctorRepository.UpdateDoctorAsync(doctor);

            Assert.That(updatedDoctor, Is.Not.Null);
            Assert.That(updatedDoctor.Name, Is.EqualTo("Updated"));
            Assert.That(updatedDoctor.YearsOfExperience, Is.EqualTo(6));

            var doctorInDb = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == doctor.Id);
            Assert.That(doctorInDb.Name, Is.EqualTo("Updated"));
            Assert.That(doctorInDb.YearsOfExperience, Is.EqualTo(6));
        }

        [Test]
        public async Task UpdateDoctorAsync_ShouldReturnNull_WhenDoctorDoesNotExist()
        {
            var doctor = new Doctor { Id = 999, Name = "NonExistent", Email = "nonexistent@example.com" };
            var updatedDoctor = await _doctorRepository.UpdateDoctorAsync(doctor);

            Assert.That(updatedDoctor, Is.Null);
        }

        [Test]
        public async Task SoftDeleteDoctorAsync_ShouldMarkDoctorAsDeleted()
        {
            var doctor = new Doctor { Name = "To Delete", Email = "todelete@example.com" };
            await _doctorRepository.AddDoctorAsync(doctor);

            var result = await _doctorRepository.SoftDeleteDoctorAsync(doctor.Id);

            Assert.That(result, Is.True);
            var doctorInDb = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctor.Id);
            Assert.That(doctorInDb.IsDeleted, Is.True);
        }

        [Test]
        public async Task SoftDeleteDoctorAsync_ShouldReturnFalse_WhenDoctorDoesNotExist()
        {
            var result = await _doctorRepository.SoftDeleteDoctorAsync(999);

            Assert.That(result, Is.False);
        }

        [Test]
        public void SoftDeleteDoctorAsync_ShouldThrowException_WhenFutureAppointmentsExist()
        {
            var doctor = new Doctor { Name = "Appointed Doc", Email = "appointed@example.com" };
            _context.Doctors.Add(doctor);
            _context.SaveChanges(); // Save to get ID

            _context.Appointments.Add(new Appointment
            {
                DoctorId = doctor.Id,
                AppointmentDateTime = DateTime.Now.AddDays(1),
                Status = "Scheduled",
                PatientId = 1 // Dummy patient ID
            });
            _context.SaveChanges();

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _doctorRepository.SoftDeleteDoctorAsync(doctor.Id));
            Assert.That(ex.Message, Is.EqualTo($"Cannot soft delete Doctor ID {doctor.Id} as they have active future appointments."));
        }

        [TestCase(1, true)]
        [TestCase(999, false)]
        public async Task DoctorExistsAsync_ShouldReturnCorrectResult(int id, bool expected)
        {
            await _doctorRepository.AddDoctorAsync(new Doctor { Id = 1, Name = "Exists", Email = "exists@example.com" });

            var exists = await _doctorRepository.DoctorExistsAsync(id);

            Assert.That(exists, Is.EqualTo(expected));
        }

        [Test]
        public async Task DoctorExistsAsync_ShouldReturnTrue_WhenIncludeDeletedIsTrueAndDoctorIsDeleted()
        {
            var doctor = new Doctor { Name = "Deleted", Email = "deleted@example.com", IsDeleted = true };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var exists = await _doctorRepository.DoctorExistsAsync(doctor.Id, includeDeleted: true);

            Assert.That(exists, Is.True);
        }

        [TestCase("test@example.com", true)]
        [TestCase("nonexistent@example.com", false)]
        public async Task DoctorExistsByEmailAsync_ShouldReturnCorrectResult(string email, bool expected)
        {
            await _doctorRepository.AddDoctorAsync(new Doctor { Name = "Test", Email = "test@example.com" });

            var exists = await _doctorRepository.DoctorExistsByEmailAsync(email);

            Assert.That(exists, Is.EqualTo(expected));
        }

        [Test]
        public async Task DoctorExistsByEmailAsync_ShouldReturnTrue_WhenIncludeDeletedIsTrueAndDoctorIsDeleted()
        {
            var doctor = new Doctor { Name = "Deleted", Email = "deleted@example.com", IsDeleted = true };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var exists = await _doctorRepository.DoctorExistsByEmailAsync(doctor.Email, includeDeleted: true);

            Assert.That(exists, Is.True);
        }
    }
}
