using AutoMapper;
using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.DTOs.Notifications;
using FreelanceProjectBoardApi.DTOs.Projects;
using FreelanceProjectBoardApi.Helpers;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Services.Interfaces;
using System.Globalization;

namespace FreelanceProjectBoardApi.Services.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository; // To verify client/freelancer existence
        private readonly IFreelancerProfileRepository _freelancerRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IProjectSkillRepository _projectSkillRepository;
        private readonly IFileService _fileService;
        private readonly IProposalService _proposalService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        // As postgres needs time in UTC so converting
        private const string DateTimeFormat = "dd-MM-yyyy HH:mm";
        private const string IndiaStandardTimeWindows = "India Standard Time";
        private const string IndiaStandardTimeIana = "Asia/Kolkata";

        public ProjectService(
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IFreelancerProfileRepository freelancerProfileRepository,
            ISkillRepository skillRepository,
            IProjectSkillRepository projectSkillRepository,
            IFileService fileService,
            IProposalService proposalService,
            INotificationService notificationService,
            IMapper mapper)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _freelancerRepository = freelancerProfileRepository;
            _skillRepository = skillRepository;
            _projectSkillRepository = projectSkillRepository;
            _fileService = fileService;
            _proposalService = proposalService;
            _notificationService = notificationService;
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

        public async Task<ProjectResponseDto?> CreateProjectAsync(Guid clientId, CreateProjectDto createDto)
        {
            var client = await _userRepository.GetByIdAsync(clientId);
            if (client == null || (client.Type != UserType.Client))
            {
                return null; // Client not found or not a valid type to create projects
            }
            // --- CONVERSION FROM IST STRING TO UTC DATETIME (DONE HERE) ---
            DateTime deadlineDateTimeUtc = ConvertIstStringToUtcDateTime(createDto.Deadline, nameof(createDto.Deadline));

            if (deadlineDateTimeUtc <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("deadline must be in the future.");
            }

            var project = _mapper.Map<Project>(createDto);
            project.ClientId = clientId;
            project.Deadline = deadlineDateTimeUtc;
            project.Status = ProjectStatus.Open; // Default status on creation

            var newProject = await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();

            // Handling skills: Find existing or create new skills, then add associations
            if (createDto.Skills != null && createDto.Skills.Any())
            {
                var ProjectSkillsToAdd = new List<ProjectSkill>();
                foreach (var skill in createDto.Skills)
                {
                    var existingSkill = await _skillRepository.GetSkillByNameAsync(skill.Name);
                    if (existingSkill == null)
                    {
                        existingSkill = await _skillRepository.AddAsync(_mapper.Map<Skill>(skill));
                    }

                    ProjectSkillsToAdd.Add(new ProjectSkill
                    {
                        ProjectId = newProject.Id,
                        SkillId = existingSkill.Id,
                        Project = newProject,
                        Skill = existingSkill
                    });
                }
                // Add all freelancer skills at once
                await _projectSkillRepository.AddRangeAsync(ProjectSkillsToAdd);
                await _projectSkillRepository.SaveChangesAsync();
            }

            // To-Do: Notify suitable freelancers

            var createdProject = await _projectRepository.GetProjectDetailsAsync(project.Id);
            return _mapper.Map<ProjectResponseDto>(createdProject);
        }

        public async Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id)
        {
            var project = await _projectRepository.GetProjectDetailsAsync(id);
            return project == null ? null : _mapper.Map<ProjectResponseDto>(project);
        }

        public async Task<ProjectResponseDto?> UpdateProjectAsync(Guid id, UpdateProjectDto updateDto)
        {
            var project = await _projectRepository.GetProjectDetailsAsync(id); // Loads with details to potentially update skills
            if (project == null || project.Status != ProjectStatus.Open)
            {
                // Cannot update a project that is not Open
                return null;
            }

            _mapper.Map(updateDto, project);
            if (updateDto.Deadline != null)
            {
                // --- CONVERSION FROM IST STRING TO UTC DATETIME (DONE HERE) ---
                DateTime deadlineDateTimeUtc = ConvertIstStringToUtcDateTime(updateDto.Deadline, nameof(updateDto.Deadline));

                if (deadlineDateTimeUtc <= DateTime.UtcNow)
                {
                    throw new InvalidOperationException("deadline must be in the future.");
                }
                project.Deadline = deadlineDateTimeUtc;
            }


            // Updates skills
            if (updateDto.Skills != null && updateDto.Skills.Any())
            {
                await _projectSkillRepository.DeleteProjectSkills(project.Id);
                var projectSkillsToAdd = new List<ProjectSkill>();
                foreach (var skill in updateDto.Skills)
                {
                    var existingSkill = await _skillRepository.GetSkillByNameAsync(skill.Name);
                    if (existingSkill == null)
                    {
                        existingSkill = await _skillRepository.AddAsync(_mapper.Map<Skill>(skill));
                    }
                    projectSkillsToAdd.Add(new ProjectSkill
                    {
                        ProjectId = project.Id,
                        Project = project,
                        Skill = existingSkill,
                        SkillId = existingSkill.Id
                    });
                }
                await _projectSkillRepository.AddRangeAsync(projectSkillsToAdd);
                await _projectSkillRepository.SaveChangesAsync();
            }

            // Handles assigned freelancer if it's explicitly set in the updateDto
            if (updateDto.AssignedFreelancerId.HasValue && project.AssignedFreelancerId != updateDto.AssignedFreelancerId.Value)
            {
                // Only allow assignment if project is open and freelancer exists
                var freelancer = await _userRepository.GetByIdAsync(updateDto.AssignedFreelancerId.Value);
                if (freelancer != null && (freelancer.Type == UserType.Freelancer))
                {
                    project.AssignedFreelancerId = updateDto.AssignedFreelancerId.Value;
                    project.Status = ProjectStatus.Assigned; // Changes status to Assigned
                }
                else
                {
                    // For now, just ignoring
                }
            }
            else if (updateDto.AssignedFreelancerId == null && project.AssignedFreelancerId.HasValue)
            {
                // If updateDto explicitly sets AssignedFreelancerId to null, unassign
                project.AssignedFreelancerId = null;
                project.Status = ProjectStatus.Open; // Reverts to Open if unassigned
            }


            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveChangesAsync();

            var updatedProject = await _projectRepository.GetProjectDetailsAsync(project.Id);
            return _mapper.Map<ProjectResponseDto>(updatedProject);
        }

        public async Task<bool> DeleteProjectAsync(Guid id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return false;
            }

            await _projectRepository.DeleteAsync(id); // Soft delete
            await _projectRepository.SaveChangesAsync();
            return true;
        }

        public async Task<PageResult<ProjectListDto>> GetAllProjectsAsync(ProjectFilter filter, PaginationParams pagination)
        {
            var pagedResult = await _projectRepository.GetAllProjectsAsync(filter, pagination);
            var projectDtos = _mapper.Map<IEnumerable<ProjectListDto>>(pagedResult.Data);

            return new PageResult<ProjectListDto>
            {
                Data = projectDtos,
                pagination = pagedResult.pagination
            };
        }

        public async Task<ProjectResponseDto?> AssignFreelancerToProjectAsync(Guid projectId, Guid freelancerId)
        {
            var project = await _projectRepository.GetProjectDetailsAsync(projectId);
            if (project == null || project.Status != ProjectStatus.Open)
            {
                return null; // Project not found or not in a state to be assigned
            }

            var freelancer = await _userRepository.GetByIdAsync(freelancerId);
            if (freelancer == null || (freelancer.Type != UserType.Freelancer))
            {
                return null; // Freelancer not found or not a valid type
            }

            var proposalID = project.Proposals?.Where(p => p.FreelancerId == freelancerId).FirstOrDefault();

            if (proposalID == null)
            {
                return null;
            }

            project.AssignedFreelancerId = freelancerId;
            project.Status = ProjectStatus.Assigned;

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveChangesAsync();

            //await _proposalService.UpdateProposalStatusAsync(proposalID.Id, new DTOs.Proposals.UpdateProposalStatusDto { NewStatus = "Accepted" });
            System.Console.WriteLine("Hit123");
            foreach (Proposal currentProposal in project.Proposals!)
            {
                if (currentProposal != null && currentProposal.FreelancerId != freelancerId)
                {
                    await _proposalService.UpdateProposalStatusAsync(currentProposal.Id, new DTOs.Proposals.UpdateProposalStatusDto { NewStatus = "Rejected" });
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        ReceiverId = currentProposal.FreelancerId,
                        Category = NotificationCategory.General,
                        Message = $"'{project.Title}' has been assigned with a differnt Freelancer. Better luck next time!"
                    });
                }
            }

            return _mapper.Map<ProjectResponseDto>(project);
        }

        public async Task<ProjectResponseDto?> MarkProjectAsCompletedAsync(Guid projectId)
        {
            var project = await _projectRepository.GetProjectDetailsAsync(projectId);
            if (project == null || project.Status != ProjectStatus.Assigned)
            {
                return null; // Project not found or not in Assigned state
            }

            project.Status = ProjectStatus.Completed;
            project.CompletionDate = DateTime.UtcNow;

            var freelancerId = project.AssignedFreelancerId;

            if (freelancerId != null && freelancerId.HasValue)
            {
                var freelancer = await _freelancerRepository.GetByIdAsync(freelancerId.Value);
                if (freelancer != null)
                {
                    freelancer.ProjectsCompleted += 1;
                    await _freelancerRepository.UpdateAsync(freelancer);
                    await _freelancerRepository.SaveChangesAsync();
                }

            }

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveChangesAsync();
            if (project.AssignedFreelancerId != null)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    ReceiverId = project.AssignedFreelancer!.Id,
                    Category = NotificationCategory.General,
                    Message = $"'{project.Title}' has been marked as completed."
                });
            }
            return _mapper.Map<ProjectResponseDto>(project);
        }

        public async Task<ProjectResponseDto?> CancelProjectAsync(Guid projectId)
        {
            var project = await _projectRepository.GetProjectDetailsAsync(projectId);
            // Allows cancellation if Open or Assigned
            if (project == null || (project.Status != ProjectStatus.Open && project.Status != ProjectStatus.Assigned))
            {
                return null; // Project not found or cannot be cancelled in its current state
            }

            project.Status = ProjectStatus.Cancelled;

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveChangesAsync();
            if (project.AssignedFreelancerId != null)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    ReceiverId = project.AssignedFreelancer!.Id,
                    Category = NotificationCategory.General,
                    Message = $"'{project.Title}' has been cancelled by client."
                });
            }
            
            return _mapper.Map<ProjectResponseDto>(project);
        }


        private async Task AddSkillsToProject(Guid projectId, List<Guid> skillIds)
        {
            var existingSkills = await _projectSkillRepository.GetSkillsForProjectAsync(projectId);
            var existingSkillIds = existingSkills.Select(ps => ps.SkillId).ToList();

            foreach (var skillId in skillIds.Distinct())
            {
                if (!existingSkillIds.Contains(skillId))
                {
                    var skillExists = await _skillRepository.GetByIdAsync(skillId);
                    if (skillExists != null)
                    {
                        await _projectSkillRepository.AddAsync(new ProjectSkill
                        {
                            ProjectId = projectId,
                            SkillId = skillId,
                            Project = (await _projectRepository.GetByIdAsync(projectId))!,
                            Skill = skillExists
                        });
                    }
                }
            }
        }

        private async Task UpdateSkillsForProject(Guid projectId, List<Guid> newSkillIds)
        {
            var currentProjectSkills = (await _projectSkillRepository.GetSkillsForProjectAsync(projectId)).ToList();
            var currentSkillIds = currentProjectSkills.Select(ps => ps.SkillId).ToList();

            // Skills to add
            var skillsToAdd = newSkillIds.Except(currentSkillIds).ToList();
            foreach (var skillId in skillsToAdd)
            {
                var skillExists = await _skillRepository.GetByIdAsync(skillId);
                if (skillExists != null)
                {
                    await _projectSkillRepository.AddAsync(new ProjectSkill
                    {
                        ProjectId = projectId,
                        SkillId = skillId,
                        Project = (await _projectRepository.GetByIdAsync(projectId))!,
                        Skill = skillExists
                    });
                }
            }

            // Skills to remove (soft delete)
            var skillsToRemove = currentSkillIds.Except(newSkillIds).ToList();
            foreach (var skillId in skillsToRemove)
            {
                var projectSkillToDelete = currentProjectSkills.FirstOrDefault(ps => ps.SkillId == skillId);
                if (projectSkillToDelete != null)
                {
                    await _projectSkillRepository.DeleteAsync(projectSkillToDelete.Id); // Soft delete the join entry
                }
            }
        }

        public async Task<FileResponseDto?> UploadProjectAttachmentAsync(Guid projectId, Guid uploaderId, IFormFile file)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return null; // Project not found
            }

            // The FileService will handle the actual file storage and database entry,
            // setting the ProjectId on the File entity.
            var uploadedFileDto = await _fileService.UploadFileAsync(file, uploaderId, FileCategory.ProjectAttachment, projectId);

            // No need to explicitly update the project entity here as the File entity's ProjectId is set.
            // So If the Project entity was loaded with Attachments, EF Core will handle the relationship.
            return uploadedFileDto;
        }

        public async Task<bool> RemoveProjectAttachmentAsync(Guid projectId, Guid attachmentId)
        {
            var project = await _projectRepository.GetProjectDetailsAsync(projectId); // Load with attachments
            if (project == null)
            {
                return false; // Project not found
            }

            // Verify the attachment belongs to this project
            var attachmentToRemove = project.Attachments?.FirstOrDefault(a => a.Id == attachmentId);
            if (attachmentToRemove == null)
            {
                return false; // Attachment not found or not associated with this project
            }

            // Uses the FileService to delete the file
            var success = await _fileService.DeleteFileAsync(attachmentId);

            return success;
        }

        public async Task<IEnumerable<FileResponseDto>> GetProjectAttachmentsMetadataAsync(Guid projectId)
        {
            var project = await _projectRepository.GetProjectDetailsAsync(projectId); // Load project with Attachments
            if (project == null || project.Attachments == null)
            {
                return Enumerable.Empty<FileResponseDto>();
            }

            // Maps the collection of File models to FileResponseDto
            return _mapper.Map<IEnumerable<FileResponseDto>>(project.Attachments.Where(f => !f.IsDeleted));
        }

        public async Task<FileResponseDto?> GetProjectAttachmentMetadataAsync(Guid attachmentId)
        {
            // Uses FileService to get the metadata of a specific file
            return await _fileService.GetFileMetadataAsync(attachmentId);
        }

        public async Task<IEnumerable<ProjectResponseDto>> GetMyProjectsAsync(Guid userId)
        {
            var projects = await _projectRepository.GetProjectsByUser(userId);
            var projectDtos = _mapper.Map<IEnumerable<ProjectResponseDto>>(projects);

            return projectDtos;
        }

    }
}
