using System.Globalization;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IMapper _mapper;

    // As postgres needs time in UTC so converting
    private const string DateTimeFormat = "dd-MM-yyyy hh:mm";
    private const string IndiaStandardTimeWindows = "India Standard Time";
    private const string IndiaStandardTimeIana = "Asia/Kolkata";


    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository,
        IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
        _mapper = mapper;
    }

    // Helper method
    private DateTime ConvertIstStringToUtcDateTime(string dateTimeString, string parameterName)
    {
        if (!DateTime.TryParseExact(dateTimeString, DateTimeFormat, CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out DateTime istDateTime))
        {
            // This should ideally be caught by DTO RegularExpression, but good for safety ;)
            throw new ArgumentException($"Invalid date/time format for {parameterName}: '{dateTimeString}'. Expected '{DateTimeFormat}'.");
        }

        TimeZoneInfo istTimeZone;
        try
        {
            istTimeZone = TimeZoneInfo.FindSystemTimeZoneById(IndiaStandardTimeWindows);
        }
        catch (TimeZoneNotFoundException)
        {
            istTimeZone = TimeZoneInfo.FindSystemTimeZoneById(IndiaStandardTimeIana);
        }

        return TimeZoneInfo.ConvertTimeToUtc(istDateTime, istTimeZone);
    }


    public async Task<AppointmentResponseDto?> GetAppointmentByNumberAsync(string appointmentNumber, bool includeDeleted = false)
    {
        var appointment = await _appointmentRepository.GetAppointmentByNumberAsync(appointmentNumber, includeDeleted);
        return _mapper.Map<AppointmentResponseDto>(appointment);
    }

    public async Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync(bool includeDeleted = false)
    {
        var appointments = await _appointmentRepository.GetAllAppointmentsAsync(includeDeleted);
        return _mapper.Map<IEnumerable<AppointmentResponseDto>>(appointments);
    }

    public async Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByPatientIdAsync(int patientId, bool includeDeleted = false)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByPatientIdAsync(patientId, includeDeleted);
        return _mapper.Map<IEnumerable<AppointmentResponseDto>>(appointments);
    }

    public async Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByDoctorIdAsync(int doctorId, bool includeDeleted = false)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByDoctorIdAsync(doctorId, includeDeleted);
        return _mapper.Map<IEnumerable<AppointmentResponseDto>>(appointments);
    }

    public async Task<AppointmentResponseDto> AddAppointmentAsync(AppointmentCreateDto appointmentDto)
    {
        var patientExists = await _patientRepository.PatientExistsAsync(appointmentDto.PatientId);
        var doctorExists = await _doctorRepository.DoctorExistsAsync(appointmentDto.DoctorId);

        if (!patientExists)
        {
            throw new ArgumentException($"Patient with ID {appointmentDto.PatientId} not found or is deleted.");
        }
        if (!doctorExists)
        {
            throw new ArgumentException($"Doctor with ID {appointmentDto.DoctorId} not found or is deleted.");
        }

        // --- CONVERSION FROM IST STRING TO UTC DATETIME (DONE HERE) ---
        DateTime appointmentDateTimeUtc = ConvertIstStringToUtcDateTime(appointmentDto.AppointmentDateTimeString, nameof(appointmentDto.AppointmentDateTimeString));

        if (appointmentDateTimeUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Appointment time must be in the future.");
        }

    
        var appointment = _mapper.Map<Appointment>(appointmentDto);

        appointment.AppointmentDateTime = appointmentDateTimeUtc;

        appointment.AppointmentNumber = Guid.NewGuid().ToString();

        var addedAppointment = await _appointmentRepository.AddAppointmentAsync(appointment);
        return _mapper.Map<AppointmentResponseDto>(addedAppointment);
    }

    public async Task<bool> SoftDeleteAppointmentAsync(string appointmentNumber)
    {
        var appointment = await _appointmentRepository.GetAppointmentByNumberAsync(appointmentNumber);
        if (appointment == null)
        {
            return false;
        }

        if (appointment.Status == "Completed")
        {
            throw new InvalidOperationException($"Cannot delete completed appointment {appointmentNumber}.");
        }

        return await _appointmentRepository.SoftDeleteAppointmentAsync(appointmentNumber);
    }

    public async Task<AppointmentResponseDto?> RescheduleAppointmentAsync(string appointmentNumber, AppointmentRescheduleDto rescheduleDto)
    {
        var existingAppointment = await _appointmentRepository.GetAppointmentByNumberAsync(appointmentNumber, includeDeleted: false);
        if (existingAppointment == null)
        {
            return null;
        }

        if (existingAppointment.Status == "Cancelled" || existingAppointment.Status == "Completed")
        {
            throw new InvalidOperationException($"Cannot reschedule appointment {appointmentNumber} with status '{existingAppointment.Status}'.");
        }

         // --- CONVERSION FROM IST STRING TO UTC DATETIME ---
        DateTime newTimingUtc = ConvertIstStringToUtcDateTime(rescheduleDto.NewTimingString, nameof(rescheduleDto.NewTimingString));
        if (newTimingUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("New appointment timing must be in the future.");
        }

        var updatedAppointment = await _appointmentRepository.RescheduleAppointmentAsync(appointmentNumber, newTimingUtc);
        return _mapper.Map<AppointmentResponseDto>(updatedAppointment);
    }

    public async Task<AppointmentResponseDto?> ChangeAppointmentStatusAsync(string appointmentNumber, AppointmentStatusChangeDto statusChangeDto)
    {
        var existingAppointment = await _appointmentRepository.GetAppointmentByNumberAsync(appointmentNumber, includeDeleted: false);
        if (existingAppointment == null)
        {
            return null;
        }

        if (statusChangeDto.NewStatus == "Cancelled")
        {
            throw new InvalidOperationException("Use the cancel endpoint to cancel a appointment");
        }

        switch (existingAppointment.Status)
            {
                case "Scheduled":
                    if (statusChangeDto.NewStatus != "Confirmed" && statusChangeDto.NewStatus != "Cancelled")
                    {
                        throw new InvalidOperationException($"Cannot change status from 'Scheduled' to '{statusChangeDto.NewStatus}'. Valid transitions are 'Confirmed' or 'Cancelled'.");
                    }
                    break;
                case "Confirmed":
                    if (statusChangeDto.NewStatus != "Completed" && statusChangeDto.NewStatus != "Cancelled")
                    {
                        throw new InvalidOperationException($"Cannot change status from 'Confirmed' to '{statusChangeDto.NewStatus}'. Valid transitions are 'Completed' or 'Cancelled'.");
                    }
                    break;
                case "Cancelled":
                case "Completed":
                    throw new InvalidOperationException($"Cannot change status of an appointment that is already '{existingAppointment.Status}'.");
            }

        var updatedAppointment = await _appointmentRepository.ChangeAppointmentStatusAsync(appointmentNumber, statusChangeDto.NewStatus);
        return _mapper.Map<AppointmentResponseDto>(updatedAppointment);
    }

    public async Task<AppointmentResponseDto?> CancelAppointment(string appointmentNumber)
    {
        var existingAppointment = await _appointmentRepository.GetAppointmentByNumberAsync(appointmentNumber, includeDeleted: false);
        if (existingAppointment == null)
        {
            return null;
        }

        if (existingAppointment.Status == "Cancelled" || existingAppointment.Status == "Completed")
        {
            throw new InvalidOperationException($"Cannot Cancel an appointment that is already '{existingAppointment.Status}'.");
        }

        var updatedAppointment = await _appointmentRepository.ChangeAppointmentStatusAsync(appointmentNumber, "Cancelled");
        return _mapper.Map<AppointmentResponseDto>(updatedAppointment);
    } 

    public async Task<bool> AppointmentExistsAsync(string appointmentNumber, bool includeDeleted = false)
    {
        return await _appointmentRepository.AppointmentExistsAsync(appointmentNumber, includeDeleted);
    }
}
