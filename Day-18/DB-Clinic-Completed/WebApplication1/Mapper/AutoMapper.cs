// dotnet add package AutoMapper
// dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

using System.Globalization;
using AutoMapper;

public static class TimeZoneConstants
{
    // On Windows systems
    public const string IndiaStandardTimeWindows = "India Standard Time";
    // On Linux/macOS systems
    public const string IndiaStandardTimeIana = "Asia/Kolkata";
}


public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Patient Mappings
        CreateMap<PatientCreateDto, Patient>();
        CreateMap<PatientUpdateDto, Patient>();
        CreateMap<Patient, PatientResponseDto>();

        // Doctor Mappings
        CreateMap<DoctorCreateDto, Doctor>();
        CreateMap<DoctorUpdateDto, Doctor>();
        CreateMap<Doctor, DoctorResponseDto>()
            .ForMember(dest => dest.Specialities, opt => opt.MapFrom(src => src.DoctorSpecialities != null ? src.DoctorSpecialities.Select(ds => ds.Speciality) : new List<Speciality>()));

        // Speciality Mappings
        CreateMap<SpecialityCreateDto, Speciality>();
        CreateMap<SpecialityUpdateDto, Speciality>();
        CreateMap<Speciality, SpecialityResponseDto>();

        // Appointment Mappings
        CreateMap<AppointmentCreateDto, Appointment>();
        CreateMap<AppointmentRescheduleDto, Appointment>();
        CreateMap<AppointmentStatusChangeDto, Appointment>();
        CreateMap<Appointment, AppointmentResponseDto>()
            // Convert the UTC AppointmentDateTime from the entity to IST for the DTO
            .ForMember(dest => dest.AppointmentDateTime, opt => opt.MapFrom(src => ConvertUtcToIst(src.AppointmentDateTime)));


        //DoctorSpeciality mappings
        CreateMap<DoctorSpecialityCreateDto, DoctorSpeciality>();
        CreateMap<DoctorSpeciality, DoctorSpecialityResponseDto>()
            .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.Name : ""))
            .ForMember(dest => dest.SpecialityName, opt => opt.MapFrom(src => src.Speciality != null ? src.Speciality.Name : ""));

    }

    private DateTime ConvertIstToUtc(string dateTimeString)
    {
        const string format = "dd-MM-yyyy hh:mm";

        if (DateTime.TryParseExact(dateTimeString, format, CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out DateTime istDateTime))
        {
            TimeZoneInfo istTimeZone;
            try
            {
                istTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConstants.IndiaStandardTimeWindows);
            }
            catch (TimeZoneNotFoundException)
            {
                istTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConstants.IndiaStandardTimeIana);
            }

            return TimeZoneInfo.ConvertTimeToUtc(istDateTime, istTimeZone);
        }
        else
        {
            throw new FormatException($"Invalid date/time format: '{dateTimeString}'. Expected '{format}'.");
        }
    }

    private DateTime ConvertUtcToIst(DateTime utcDateTime)
    {
        TimeZoneInfo istTimeZone;
        try
        {
            istTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConstants.IndiaStandardTimeWindows);
        }
        catch (TimeZoneNotFoundException)
        {
            istTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConstants.IndiaStandardTimeIana);
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, istTimeZone);
    }

}