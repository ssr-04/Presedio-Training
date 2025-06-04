using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Auth0.ManagementApi.Models;
using Microsoft.EntityFrameworkCore; // Required for in-memory database
using WebApplication1.Services; 
using WebApplication1.Interaces; 
namespace ClinicApi.Tests.Services
{
    [TestFixture]
    public class DoctorServiceTests
    {
        // Keep mocks for external services and other dependencies
        private Mock<ISpecialityService> _mockSpecialityService;
        private Mock<IDoctorSpecialityService> _mockDoctorSpecialityService;
        private Mock<IOtherContextFunctionalities> _mockOtherContextFunctionalities;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IAuth0ManagementService> _mockAuth0ManagementService;
        private Mock<IMapper> _mockMapper;

        // Real instances for repository and context
        private ClinicContext _context;
        private DoctorRepository _doctorRepository;

        private DoctorService _doctorService;

        [SetUp]
        public void Setup()
        {
            // Setup in-memory database for DoctorRepository
            var options = new DbContextOptionsBuilder<ClinicContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
            _context = new ClinicContext(options);
            _context.Database.EnsureCreated(); // Ensure the in-memory database is created

            _doctorRepository = new DoctorRepository(_context); // Real DoctorRepository instance

            // Initialize mocks for other dependencies
            _mockSpecialityService = new Mock<ISpecialityService>();
            _mockDoctorSpecialityService = new Mock<IDoctorSpecialityService>();
            _mockOtherContextFunctionalities = new Mock<IOtherContextFunctionalities>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuth0ManagementService = new Mock<IAuth0ManagementService>();
            _mockMapper = new Mock<IMapper>();

            // Instantiate DoctorService with real DoctorRepository and mocked dependencies
            _doctorService = new DoctorService(
                _doctorRepository, // Use the real in-memory repository
                _mockSpecialityService.Object,
                _mockDoctorSpecialityService.Object,
                _mockOtherContextFunctionalities.Object,
                _mockUserRepository.Object,
                _mockAuth0ManagementService.Object,
                _mockMapper.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted(); // Clean up in-memory database
            _context.Dispose(); // Dispose the context
        }

        [Test]
        public async Task GetDoctorByIdAsync_ShouldReturnDoctorResponseDto_WhenDoctorExists()
        {
            // Arrange
            var doctor = new Doctor { Id = 1, Name = "Dr. Smith", Email = "smith@example.com" };
            // Add doctor directly to the in-memory context to be retrieved by the real repository
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var doctorDto = new DoctorResponseDto { Id = 1, Name = "Dr. Smith", Email = "smith@example.com" };

            // Mock only the mapper, as the repository call is real
            _mockMapper.Setup(m => m.Map<DoctorResponseDto>(doctor)).Returns(doctorDto);

            // Act
            var result = await _doctorService.GetDoctorByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Dr. Smith"));
            // No Verify on _mockDoctorRepository, as we're using the real one
            _mockMapper.Verify(m => m.Map<DoctorResponseDto>(doctor), Times.Once);
        }

        [Test]
        public async Task GetDoctorByIdAsync_ShouldReturnNull_WhenDoctorDoesNotExist()
        {
            // Arrange - No doctor added to context

            // Act
            var result = await _doctorService.GetDoctorByIdAsync(999);

            // Assert
            Assert.That(result, Is.Null);
            // No Verify on _mockDoctorRepository, as we're using the real one
            _mockMapper.Verify(m => m.Map<DoctorResponseDto>(It.IsAny<Doctor>()), Times.Never);
        }

        [Test]
        public async Task GetAllDoctorsAsync_ShouldReturnAllDoctors()
        {
            // Arrange
            var doctors = new List<Doctor> { new Doctor { Id = 1, Name = "Dr. A" }, new Doctor { Id = 2, Name = "Dr. B" } };
            _context.Doctors.AddRange(doctors);
            await _context.SaveChangesAsync();

            var doctorDtos = new List<DoctorResponseDto> { new DoctorResponseDto { Id = 1, Name = "Dr. A" }, new DoctorResponseDto { Id = 2, Name = "Dr. B" } };

            // Mock only the mapper
            _mockMapper.Setup(m => m.Map<IEnumerable<DoctorResponseDto>>(It.IsAny<IEnumerable<Doctor>>())).Returns(doctorDtos);

            // Act
            var result = await _doctorService.GetAllDoctorsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            // No Verify on _mockDoctorRepository
            _mockMapper.Verify(m => m.Map<IEnumerable<DoctorResponseDto>>(It.IsAny<IEnumerable<Doctor>>()), Times.Once);
        }

        [Test]
        public async Task AddDoctorAsync_ShouldAddDoctorAndAuth0User_Successfully()
        {
            // Arrange
            var doctorDto = new DoctorCreateDto { Email = "newdoctor@example.com", Password = "Password123!", Name = "New Doctor" };
            var auth0User = new User { UserId = "auth0|123", Email = "newdoctor@example.com" };
            var doctor = new Doctor { Id = 1, Email = "newdoctor@example.com", Name = "New Doctor" };
            // localUser will be created by the service

            var doctorResponseDto = new DoctorResponseDto { Id = 1, Email = "newdoctor@example.com", Name = "New Doctor" };

            _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
            _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email)).ReturnsAsync((Auth0.ManagementApi.Models.User)null);
            _mockAuth0ManagementService.Setup(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor")).ReturnsAsync(auth0User);
            // No need to mock DoctorExistsByEmailAsync or AddDoctorAsync for _doctorRepository, as we're using the real one
            _mockMapper.Setup(m => m.Map<Doctor>(doctorDto)).Returns(doctor); // Map DTO to entity for repo
            _mockUserRepository.Setup(r => r.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<DoctorResponseDto>(It.IsAny<Doctor>())).Returns(doctorResponseDto); // Map entity to response DTO

            // Act
            var result = await _doctorService.AddDoctorAsync(doctorDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo(doctorDto.Email));
            _mockUserRepository.Verify(r => r.UserExistsByUsernameAsync(doctorDto.Email), Times.Once);
            _mockAuth0ManagementService.Verify(s => s.GetAuth0UserByEmailAsync(doctorDto.Email), Times.Once);
            _mockAuth0ManagementService.Verify(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor"), Times.Once);
            // Verify that the real repository methods were called implicitly
            var addedDoctorInDb = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == doctorDto.Email);
            Assert.That(addedDoctorInDb, Is.Not.Null);
            var addedUserInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == doctorDto.Email);
            Assert.That(addedUserInDb, Is.Not.Null);
            Assert.That(addedUserInDb.Auth0UserId, Is.EqualTo(auth0User.UserId));
            Assert.That(addedUserInDb.DoctorId, Is.EqualTo(addedDoctorInDb.Id));

            _mockUserRepository.Verify(r => r.AddUserAsync(It.Is<User>(u => u.Auth0UserId == auth0User.UserId && u.Username == doctorDto.Email && u.DoctorId == addedDoctorInDb.Id)), Times.Once);
            _mockMapper.Verify(m => m.Map<Doctor>(doctorDto), Times.Once);
            _mockMapper.Verify(m => m.Map<DoctorResponseDto>(It.IsAny<Doctor>()), Times.Once);
        }

        [Test]
        public void AddDoctorAsync_ShouldThrowException_WhenLocalUserEmailExists()
        {
            // Arrange
            var doctorDto = new DoctorCreateDto { Email = "existing@example.com", Password = "Password123!", Name = "Existing Doctor" };
            _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(true);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _doctorService.AddDoctorAsync(doctorDto));
            Assert.That(ex.Message, Is.EqualTo($"A user account with email '{doctorDto.Email}' already exists."));

            _mockUserRepository.Verify(r => r.UserExistsByUsernameAsync(doctorDto.Email), Times.Once);
            _mockAuth0ManagementService.Verify(s => s.GetAuth0UserByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockAuth0ManagementService.Verify(s => s.CreateAuth0UserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void AddDoctorAsync_ShouldThrowException_WhenAuth0UserEmailExists()
        {
            // Arrange
            var doctorDto = new DoctorCreateDto { Email = "existing@example.com", Password = "Password123!", Name = "Existing Doctor" };
            _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
            _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email)).ReturnsAsync(new User { Email = doctorDto.Email });

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _doctorService.AddDoctorAsync(doctorDto));
            Assert.That(ex.Message, Is.EqualTo($"A user with email '{doctorDto.Email}' already exists in Auth0."));

            _mockUserRepository.Verify(r => r.UserExistsByUsernameAsync(doctorDto.Email), Times.Once);
            _mockAuth0ManagementService.Verify(s => s.GetAuth0UserByEmailAsync(doctorDto.Email), Times.Once);
            _mockAuth0ManagementService.Verify(s => s.CreateAuth0UserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void AddDoctorAsync_ShouldThrowException_WhenAuth0UserCreationFails()
        {
            // Arrange
            var doctorDto = new DoctorCreateDto { Email = "newdoctor@example.com", Password = "Password123!", Name = "New Doctor" };
            _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
            _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email)).ReturnsAsync((Auth0.ManagementApi.Models.User)null);
            _mockAuth0ManagementService.Setup(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor")).ThrowsAsync(new Exception("Auth0 API Error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _doctorService.AddDoctorAsync(doctorDto));
            Assert.That(ex.Message, Does.StartWith("Failed to create user in Auth0:"));

            _mockUserRepository.Verify(r => r.UserExistsByUsernameAsync(doctorDto.Email), Times.Once);
            _mockAuth0ManagementService.Verify(s => s.GetAuth0UserByEmailAsync(doctorDto.Email), Times.Once);
            _mockAuth0ManagementService.Verify(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor"), Times.Once);
            // No Verify on _doctorRepository methods as they shouldn't be called
        }

        [Test]
        public async Task UpdateDoctorAsync_ShouldUpdateDoctorSuccessfully()
        {
            // Arrange
            var existingDoctor = new Doctor { Id = 1, Name = "Old Name", Email = "old@example.com", Status = "Active" };
            _context.Doctors.Add(existingDoctor);
            await _context.SaveChangesAsync(); // Ensure doctor exists in DB

            var doctorDto = new DoctorUpdateDto { Name = "New Name", Email = "old@example.com", Status = "Active" };
            var updatedDoctorEntity = new Doctor { Id = 1, Name = "New Name", Email = "old@example.com", Status = "Active" }; // The entity after mapping
            var doctorResponseDto = new DoctorResponseDto { Id = 1, Name = "New Name", Email = "old@example.com", Status = "Active" };

            // Mock mapper to simulate DTO to entity mapping
            _mockMapper.Setup(m => m.Map(doctorDto, It.IsAny<Doctor>()))
                       .Callback<DoctorUpdateDto, Doctor>((dto, entity) =>
                       {
                           entity.Name = dto.Name;
                           entity.Status = dto.Status;
                           entity.YearsOfExperience = dto.YearsOfExperience;
                           entity.Email = dto.Email;
                           entity.Phone = dto.Phone;
                       });
            _mockMapper.Setup(m => m.Map<DoctorResponseDto>(It.IsAny<Doctor>())).Returns(doctorResponseDto);

            // Act
            var result = await _doctorService.UpdateDoctorAsync(1, doctorDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("New Name"));
            // Verify the real repository updated the entity in DB
            var doctorInDb = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == 1);
            Assert.That(doctorInDb.Name, Is.EqualTo("New Name"));

            _mockMapper.Verify(m => m.Map(doctorDto, It.IsAny<Doctor>()), Times.Once);
            _mockMapper.Verify(m => m.Map<DoctorResponseDto>(It.IsAny<Doctor>()), Times.Once);
        }

        [Test]
        public async Task UpdateDoctorAsync_ShouldReturnNull_WhenDoctorDoesNotExist()
        {
            // Arrange - No doctor with ID 999 in DB

            var doctorDto = new DoctorUpdateDto { Name = "New Name", Email = "test@example.com" };

            // Act
            var result = await _doctorService.UpdateDoctorAsync(999, doctorDto);

            // Assert
            Assert.That(result, Is.Null);
            _mockMapper.Verify(m => m.Map(It.IsAny<DoctorUpdateDto>(), It.IsAny<Doctor>()), Times.Never);
        }

        [Test]
        public void UpdateDoctorAsync_ShouldThrowException_WhenNewEmailAlreadyExists()
        {
            // Arrange
            var existingDoctor = new Doctor { Id = 1, Name = "Old Name", Email = "old@example.com", Status = "Active" };
            _context.Doctors.Add(existingDoctor);
            _context.Doctors.Add(new Doctor { Id = 2, Name = "Other Doctor", Email = "new@example.com", Status = "Active" });
            _context.SaveChanges();

            var doctorDto = new DoctorUpdateDto { Name = "New Name", Email = "new@example.com", Status = "Active" }; // Email changed to an existing one

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _doctorService.UpdateDoctorAsync(1, doctorDto));
            Assert.That(ex.Message, Is.EqualTo($"Doctor with email '{doctorDto.Email}' already exists."));

            // Verify that the repository was called to check email existence
            // No mapper verification as it should throw before mapping
        }

        [Test]
        public async Task SoftDeleteDoctorAsync_ShouldMarkDoctorAndUserAsInactive()
        {
            // Arrange
            var doctor = new Doctor { Id = 1, Email = "todelete@example.com", Name = "Dr. Delete" };
            var user = new User { Id = 10, Username = "todelete@example.com", DoctorId = 1, IsActive = true };
            _context.Doctors.Add(doctor);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Mock user repository update
            _mockUserRepository.Setup(r => r.GetUserByUsernameAsync(doctor.Email)).ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            var result = await _doctorService.SoftDeleteDoctorAsync(1);

            // Assert
            Assert.That(result, Is.True);
            // Verify real doctor is soft-deleted
            var doctorInDb = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == 1);
            Assert.That(doctorInDb.IsDeleted, Is.True);
            // Verify user is updated
            _mockUserRepository.Verify(r => r.GetUserByUsernameAsync(doctor.Email), Times.Once);
            _mockUserRepository.Verify(r => r.UpdateUserAsync(It.Is<User>(u => u.IsActive == false && u.Id == user.Id)), Times.Once);
        }

        [Test]
        public async Task SoftDeleteDoctorAsync_ShouldReturnFalse_WhenDoctorDoesNotExist()
        {
            // Arrange - No doctor with ID 999 in DB

            // Act
            var result = await _doctorService.SoftDeleteDoctorAsync(999);

            // Assert
            Assert.That(result, Is.False);
            // No calls to user repository or soft delete on doctor repository
            _mockUserRepository.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task AddDoctorAsync_ShouldHandleSpecialities_Successfully()
        {
            // Arrange
            var doctorDto = new DoctorCreateDto
            {
                Email = "specdoctor@example.com",
                Password = "Password123!",
                Name = "Speciality Doctor",
                SpecialityNames = new List<SpecialityCreateDto>
                {
                    new SpecialityCreateDto { Name = "Cardiology" },
                    new SpecialityCreateDto { Name = "Neurology" }
                }
            };
            var auth0User = new User { UserId = "auth0|spec", Email = "specdoctor@example.com" };
            var doctor = new Doctor { Id = 1, Email = "specdoctor@example.com", Name = "Speciality Doctor" };
            var localUser = new User { Id = 1, Username = "specdoctor@example.com", Auth0UserId = "auth0|spec", Role = "Doctor" };
            var doctorResponseDto = new DoctorResponseDto { Id = 1, Email = "specdoctor@example.com", Name = "Speciality Doctor" };

            var existingSpeciality = new SpecialityResponseDto { Id = 10, Name = "Cardiology" };
            var newSpeciality = new SpecialityResponseDto { Id = 11, Name = "Neurology" };

            _mockUserRepository.Setup(r => r.UserExistsByUsernameAsync(doctorDto.Email)).ReturnsAsync(false);
            _mockAuth0ManagementService.Setup(s => s.GetAuth0UserByEmailAsync(doctorDto.Email)).ReturnsAsync((Auth0.ManagementApi.Models.User)null);
            _mockAuth0ManagementService.Setup(s => s.CreateAuth0UserAsync(doctorDto.Email, doctorDto.Password, "Doctor")).ReturnsAsync(auth0User);
            // No need to mock DoctorExistsByEmailAsync or AddDoctorAsync for real repo
            _mockMapper.Setup(m => m.Map<Doctor>(doctorDto)).Returns(doctor);
            _mockUserRepository.Setup(r => r.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            _mockSpecialityService.Setup(s => s.GetAllSpecialitiesAsync(false))
                .ReturnsAsync(new List<SpecialityResponseDto> { existingSpeciality });
            _mockSpecialityService.Setup(s => s.AddSpecialityAsync(It.Is<SpecialityCreateDto>(dto => dto.Name == "Neurology")))
                .ReturnsAsync(newSpeciality);

            _mockDoctorSpecialityService.Setup(s => s.AddDoctorSpecialityAsync(It.IsAny<DoctorSpecialityCreateDto>()))
                .Returns(Task.CompletedTask);

            _mockMapper.Setup(m => m.Map<DoctorResponseDto>(It.IsAny<Doctor>())).Returns(doctorResponseDto);


            // Act
            var result = await _doctorService.AddDoctorAsync(doctorDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            // Verify that the real repository added the doctor
            var addedDoctorInDb = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == doctorDto.Email);
            Assert.That(addedDoctorInDb, Is.Not.Null);
            // Verify speciality service interactions
            _mockSpecialityService.Verify(s => s.GetAllSpecialitiesAsync(false), Times.Once);
            _mockSpecialityService.Verify(s => s.AddSpecialityAsync(It.Is<SpecialityCreateDto>(dto => dto.Name == "Neurology")), Times.Once);
            _mockDoctorSpecialityService.Verify(s => s.AddDoctorSpecialityAsync(
                It.Is<DoctorSpecialityCreateDto>(ds => ds.DoctorId == addedDoctorInDb.Id && ds.SpecialityId == existingSpeciality.Id)), Times.Once);
            _mockDoctorSpecialityService.Verify(s => s.AddDoctorSpecialityAsync(
                It.Is<DoctorSpecialityCreateDto>(ds => ds.DoctorId == addedDoctorInDb.Id && ds.SpecialityId == newSpeciality.Id)), Times.Once);
        }

        [Test]
        public async Task DoctorExistsAsync_ShouldReturnCorrectResult()
        {
            // Arrange
            _context.Doctors.Add(new Doctor { Id = 1, Name = "Exists", Email = "exists@example.com" });
            await _context.SaveChangesAsync();

            // Act
            var exists1 = await _doctorService.DoctorExistsAsync(1);
            var exists2 = await _doctorService.DoctorExistsAsync(2);

            // Assert
            Assert.That(exists1, Is.True);
            Assert.That(exists2, Is.False);
            // No Verify on _mockDoctorRepository, as we're using the real one
        }

        [Test]
        public async Task DoctorExistsByEmailAsync_ShouldReturnCorrectResult()
        {
            // Arrange
            _context.Doctors.Add(new Doctor { Name = "Test", Email = "test@example.com" });
            await _context.SaveChangesAsync();

            // Act
            var exists1 = await _doctorService.DoctorExistsByEmailAsync("test@example.com");
            var exists2 = await _doctorService.DoctorExistsByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.That(exists1, Is.True);
            Assert.That(exists2, Is.False);
            // No Verify on _mockDoctorRepository
        }

        [Test]
        public async Task GetDoctorsBySpecialityNameAsync_ReturnsMappedDoctors_WhenSpReturnsDoctors()
        {
            string specialityName = "Cardiology";

            var doctorsFromSp = new List<Doctor>
            {
                new Doctor { Id = 1, Name = "Shaun", Email = "shaun@example.com", Status = "Active", YearsOfExperience = 10, Phone = "111-222-3333" },
                new Doctor { Id = 2, Name = "Clarie", Email = "clarie@example.com", Status = "Active", YearsOfExperience = 15, Phone = "444-555-6666" }
            };

            var expectedDoctorDtos = new List<DoctorResponseDto>
            {
                new DoctorResponseDto { Id = 1, Name = "Shaun", Email = "shaun@example.com", Status = "Active", YearsOfExperience = 10, Phone = "111-222-3333" },
                new DoctorResponseDto { Id = 2, Name = "Clarie", Email = "clarie@example.com", Status = "Active", YearsOfExperience = 15, Phone = "444-555-6666" }
            };

            _mockOtherContextFunctionalities
                .Setup(x => x.GetDoctorsBySpecialityNameFromSpAsync(specialityName))
                .ReturnsAsync(doctorsFromSp);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<DoctorResponseDto>>(It.IsAny<IEnumerable<Doctor>>()))
                .Returns(expectedDoctorDtos);

            var result = await _doctorService.GetDoctorsBySpecialityNameAsync(specialityName);

            _mockOtherContextFunctionalities.Verify(x => x.GetDoctorsBySpecialityNameFromSpAsync(specialityName), Times.Once);
            _mockMapper.Verify(m => m.Map<IEnumerable<DoctorResponseDto>>(doctorsFromSp), Times.Once);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(expectedDoctorDtos.Count));

            foreach (var expectedDto in expectedDoctorDtos)
            {
                var actualDto = result.FirstOrDefault(d => d.Id == expectedDto.Id);
                Assert.That(actualDto, Is.Not.Null);
                Assert.That(actualDto.Name, Is.EqualTo(expectedDto.Name));
                Assert.That(actualDto.Email, Is.EqualTo(expectedDto.Email));
                Assert.That(actualDto.YearsOfExperience, Is.EqualTo(expectedDto.YearsOfExperience));
            }
        }

        [Test]
        public async Task GetDoctorsBySpecialityNameAsync_ReturnsEmptyList_WhenSpReturnsNoDoctors()
        {
            string specialityName = "NonExistentSpeciality";

            var doctorsFromSp = new List<Doctor>();
            _mockOtherContextFunctionalities
                .Setup(x => x.GetDoctorsBySpecialityNameFromSpAsync(specialityName))
                .ReturnsAsync(doctorsFromSp);

            var expectedDoctorDtos = new List<DoctorResponseDto>();
            _mockMapper
                .Setup(m => m.Map<IEnumerable<DoctorResponseDto>>(It.IsAny<IEnumerable<Doctor>>()))
                .Returns(expectedDoctorDtos);

            var result = await _doctorService.GetDoctorsBySpecialityNameAsync(specialityName);

            _mockOtherContextFunctionalities.Verify(x => x.GetDoctorsBySpecialityNameFromSpAsync(specialityName), Times.Once);
            _mockMapper.Verify(m => m.Map<IEnumerable<DoctorResponseDto>>(doctorsFromSp), Times.Once);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task AddDoctorByTransactionAsync_ShouldReturnMappedDoctor()
        {
            var doctorDto = new DoctorCreateDto { Email = "transactional@example.com", Name = "Transactional Doctor" };
            var doctorResponseFromOtherContext = new DoctorResponseDto { Id = 5, Email = "transactional@example.com", Name = "Transactional Doctor" };
            var finalDoctor = new Doctor { Id = 5, Email = "transactional@example.com", Name = "Transactional Doctor" };
            var finalDoctorResponseDto = new DoctorResponseDto { Id = 5, Email = "transactional@example.com", Name = "Transactional Doctor" };


            // Mock GetDoctorByIdAsync on the real repository
            _context.Doctors.Add(finalDoctor); // Add the doctor to the in-memory DB for GetDoctorByIdAsync to find
            await _context.SaveChangesAsync();

            _mockMapper.Setup(m => m.Map<DoctorResponseDto>(finalDoctor)).Returns(finalDoctorResponseDto);

            var result = await _doctorService.AddDoctorByTransactionAsync(doctorDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(5));
            _mockOtherContextFunctionalities.Verify(o => o.AddDoctorTransactionalAsync(
                doctorDto,
                _doctorRepository,
                _mockSpecialityService.Object,
                _mockDoctorSpecialityService.Object,
                _mockMapper.Object
            ), Times.Once);
            // Verify the real repository was called
            var doctorInDb = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == 5);
            Assert.That(doctorInDb, Is.Not.Null);
            _mockMapper.Verify(m => m.Map<DoctorResponseDto>(finalDoctor), Times.Once);
        }

        [Test]
        public void AddDoctorByTransactionAsync_ShouldThrowException_WhenFinalDoctorNotFound()
        {
            var doctorDto = new DoctorCreateDto { Email = "transactional@example.com", Name = "Transactional Doctor" };
            var doctorResponseFromOtherContext = new DoctorResponseDto { Id = 5, Email = "transactional@example.com", Name = "Transactional Doctor" };

            _mockOtherContextFunctionalities.Setup(o => o.AddDoctorTransactionalAsync(
                doctorDto,
                _doctorRepository, // Use the real in-memory repository instance here
                _mockSpecialityService.Object,
                _mockDoctorSpecialityService.Object,
                _mockMapper.Object
            )).ReturnsAsync(doctorResponseFromOtherContext);

            // Do NOT add finalDoctor to context, so GetDoctorByIdAsync returns null

            var ex = Assert.ThrowsAsync<Exception>(async () => await _doctorService.AddDoctorByTransactionAsync(doctorDto));
            Assert.That(ex.Message, Is.EqualTo("Failed to get doctor with ID 5 after transaction add."));

            _mockOtherContextFunctionalities.Verify(o => o.AddDoctorTransactionalAsync(
                doctorDto,
                _doctorRepository,
                _mockSpecialityService.Object,
                _mockDoctorSpecialityService.Object,
                _mockMapper.Object
            ), Times.Once);
            // Verify the real repository was called to get the doctor, but it returned null
            _mockMapper.Verify(m => m.Map<DoctorResponseDto>(It.IsAny<Doctor>()), Times.Never);
        }
    }
}
