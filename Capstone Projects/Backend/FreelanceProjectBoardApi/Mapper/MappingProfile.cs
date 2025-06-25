using System.Globalization;
using AutoMapper;
using FreelanceProjectBoardApi.DTOs.ClientProfiles;
using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.DTOs.FreelancerProfiles;
using FreelanceProjectBoardApi.DTOs.Notifications;
using FreelanceProjectBoardApi.DTOs.Projects;
using FreelanceProjectBoardApi.DTOs.Proposals;
using FreelanceProjectBoardApi.DTOs.Ratings;
using FreelanceProjectBoardApi.DTOs.Skills;
using FreelanceProjectBoardApi.DTOs.Users;
using FreelanceProjectBoardApi.Models;

namespace FreelanceProjectBoardApi.Mapper
{
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
            // User Mappings
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.ClientProfile, opt => opt.MapFrom(src => src.ClientProfile))
                .ForMember(dest => dest.FreelancerProfile, opt => opt.MapFrom(src => src.FreelancerProfile))
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.Type.ToString()))
                .ReverseMap();
            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); //Only mapping from non null properties

            // Client Profile mappings
            CreateMap<ClientProfile, ClientProfileResponseDto>()
                .ReverseMap();
            CreateMap<CreateClientProfileDto, ClientProfile>();
            CreateMap<UpdateClientProfileDto, ClientProfile>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Freeancer profile mappings
            CreateMap<FreelancerProfile, FreelancerProfileResponseDto>()
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.FreelancerSkills!.Select(fs => fs.Skill))) // Mapping join table to list of SkillDto
                .ForMember(dest => dest.AverageRating, opt => opt.Ignore()) // Calculated in service, so ignore for mapping
                .ReverseMap();
            CreateMap<CreateFreelancerProfileDto, FreelancerProfile>()
                .ForMember(dest => dest.FreelancerSkills, opt => opt.Ignore());
            CreateMap<UpdateFreelancerProfileDto, FreelancerProfile>()
                .ForMember(dest => dest.FreelancerSkills, opt => opt.Ignore()) // Ignore SkillIds/Skills during direct mapping
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Project Mappings
            CreateMap<Project, ProjectResponseDto>()
                .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client)) // Mapping related User to UserResponseDto
                .ForMember(dest => dest.AssignedFreelancer, opt => opt.MapFrom(src => src.AssignedFreelancer)) // Mapping related User to UserResponseDto
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.SkillsRequired, opt => opt.MapFrom(src => src.ProjectSkills!.Select(ps => ps.Skill))) // Mapping join table to list of SkillDto
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments))
                // Convert the UTC AppointmentDateTime from the entity to IST for the DTO
                .ForMember(dest => dest.Deadline, opt => opt.MapFrom(src => src.Deadline.HasValue ? ConvertUtcToIst(src.Deadline.Value) : DateTime.MinValue))
                .ReverseMap();
            CreateMap<Project, ProjectListDto>()
                .ForMember(dest => dest.ClientCompanyName, opt => opt.MapFrom(src => src.Client.ClientProfile!.CompanyName))
                .ForMember(dest => dest.SkillsRequired, opt => opt.MapFrom(src => src.ProjectSkills!.Select(ps => ps.Skill)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap();
            CreateMap<CreateProjectDto, Project>()
                .ForMember(dest => dest.Deadline, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectSkills, opt => opt.Ignore()); // Skills handled separately after creation
            CreateMap<UpdateProjectDto, Project>()
                .ForMember(dest => dest.Deadline, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectSkills, opt => opt.Ignore()) // Skills handled separately for updates
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Proposal Mappings
            CreateMap<Proposal, ProposalResponseDto>()
                .ForMember(dest => dest.Project, opt => opt.MapFrom(src => src.Project)) // Mapping Project to ProjectListDto
                .ForMember(dest => dest.Freelancer, opt => opt.MapFrom(src => src.Freelancer)) // Mapping User to UserResponseDto
                .ForMember(dest => dest.ProposedDeadline, opt => opt.MapFrom(src => src.ProposedDeadLine.HasValue ? ConvertUtcToIst(src.ProposedDeadLine.Value) : DateTime.MinValue))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap();
            CreateMap<CreateProposalDto, Proposal>()
            .ForMember(dest => dest.ProposedDeadLine, opt => opt.Ignore());
            // UpdateProposalStatusDto only maps to status, handled in service directly 
            CreateMap<UpdateProposalDto, Proposal>();
                

            //File mappings
            CreateMap<Models.File, FileResponseDto>().ReverseMap();

            // Ratings mappings
            CreateMap<Rating, RatingResponseDto>()
               .ForMember(dest => dest.Project, opt => opt.MapFrom(src => src.Project)) // Mapping Project to ProjectListDto
               .ForMember(dest => dest.Rater, opt => opt.MapFrom(src => src.Rater)) // Mapping User to UserResponseDto
               .ForMember(dest => dest.Ratee, opt => opt.MapFrom(src => src.Ratee)) // Mapping User to UserResponseDto
               .ReverseMap();
            CreateMap<CreateRatingDto, Rating>();

            // Skill mappings
            CreateMap<Skill, SkillDto>().ReverseMap();
            CreateMap<CreateSkillDto, Skill>();

            // Notification Mappings
            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()));
            CreateMap<CreateNotificationDto, Notification>();

            // Join Table Mappings (for nested collections) 
            CreateMap<FreelancerSkill, SkillDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Skill.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Skill.Name));

            CreateMap<ProjectSkill, SkillDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Skill.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Skill.Name));


        }

        private DateTime ConvertIstToUtc(string dateTimeString)
        {
            const string format = "dd-MM-yyyy HH:mm";

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
}