using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace ClinicApi.Tests
{
    [TestFixture] // Marks class as it contains NUnit tests
    public class DoctorRepositoryTests
    {
        private ClinicContext _context;
        private DoctorRepository _doctorRepository;

        // Runs before each test
        [SetUp]
        public void Setup()
        {
            // Configure in-memory db for isolated testing
            // Use a unique name for each test database to ensure isolation
            var options = new DbContextOptionsBuilder<ClinicContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
            _context = new ClinicContext(options);
            _context.Database.EnsureCreated(); // Ensures in-memory database is created
            _doctorRepository = new DoctorRepository(_context);
        }

        // Will Run after each test
        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted(); // Deletes the in-memory database
            _context.Dispose(); // Disposes the context
        }

        [Test]
        public async Task AddDoctorAsync()
        {
            // Arrange
            var newDoctor = new Doctor
            {
                Name = "Shaun",
                Status = "Active",
                YearsOfExperience = 5,
                Email = "shaun@gmail.com",
                Phone = "1234567890"
                // IsDeleted is set by the repo, no need to set here
            };

            // Act
            var addedDoctor = await _doctorRepository.AddDoctorAsync(newDoctor);

            // Assert using Assert.That with constraints
            Assert.That(addedDoctor, Is.Not.Null, "Added doctor should not be null.");
            Assert.That(addedDoctor.Id, Is.GreaterThan(0), "Doctor Id should be greater than 0 after save.");
            Assert.That(addedDoctor.Name, Is.EqualTo(newDoctor.Name), "Doctor name should match.");
            Assert.That(addedDoctor.Email, Is.EqualTo(newDoctor.Email), "Doctor email should match.");
            Assert.That(addedDoctor.IsDeleted, Is.False, "IsDeleted should be false on creation.");

            // Verify (Doctor in DB)
            var doctorInDb = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == addedDoctor.Id);
            Assert.That(doctorInDb, Is.Not.Null, "Doctor should be found in the database.");
            Assert.That(doctorInDb.Name, Is.EqualTo(addedDoctor.Name), "Doctor name in DB should match.");
            Assert.That(doctorInDb.Email, Is.EqualTo(addedDoctor.Email), "Doctor email in DB should match.");
            Assert.That(doctorInDb.IsDeleted, Is.False, "IsDeleted in DB should be false on creation.");
        }

        [TestCase(1)] //Will exist
        [TestCase(2)] //Does not exist so fails
        public async Task GetDoctorByIdAsync(int id)
        {
            // Arrange
            var newDoctor = new Doctor
            {
                Name = "Shaun",
                Status = "Active",
                YearsOfExperience = 5,
                Email = "shaun@gmail.com",
                Phone = "1234567890"
                // IsDeleted is set by the repo, no need to set here
            };
            var addedDoctor = await _doctorRepository.AddDoctorAsync(newDoctor);

            // Act
            var doctor = await _doctorRepository.GetDoctorByIdAsync(id);

            // Assert
            Assert.That(doctor.Id, Is.EqualTo(id), "GetDoctorByIdAsync should return correct Id of 1 for first user.");
        }

    }
}