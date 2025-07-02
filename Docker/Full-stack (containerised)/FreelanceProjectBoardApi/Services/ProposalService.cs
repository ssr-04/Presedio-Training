using System.Globalization;
using AutoMapper;
using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.DTOs.Proposals;
using FreelanceProjectBoardApi.Interfaces.Repositories;
using FreelanceProjectBoardApi.Models;
using FreelanceProjectBoardApi.Services.Interfaces;
using Microsoft.AspNetCore.Routing.Tree;

namespace FreelanceProjectBoardApi.Services.Implementations
{
    public class ProposalService : IProposalService
    {
        private readonly IProposalRepository _proposalRepository;
        private readonly IProjectRepository _projectRepository; // To verify project status/existence
        private readonly IUserRepository _userRepository; // To verify freelancer existence
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public ProposalService(
            IProposalRepository proposalRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IFileService fileService,
            IMapper mapper)
        {
            _proposalRepository = proposalRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _fileService = fileService;
            _mapper = mapper;
        }

        // As postgres needs time in UTC so converting
        private const string DateTimeFormat = "dd-MM-yyyy HH:mm";
        private const string IndiaStandardTimeWindows = "India Standard Time";
        private const string IndiaStandardTimeIana = "Asia/Kolkata";

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

        public async Task<ProposalResponseDto?> CreateProposalAsync(Guid freelancerId, CreateProposalDto createDto)
        {
            var freelancer = await _userRepository.GetByIdAsync(freelancerId);
            if (freelancer == null || (freelancer.Type != UserType.Freelancer))
            {
                return null; // Freelancer not found or not a valid type
            }

            var project = await _projectRepository.GetByIdAsync(createDto.ProjectId);
            if (project == null || project.Status != ProjectStatus.Open)
            {
                return null; // Project not found or not open for proposals
            }

            // Check if freelancer already submitted a proposal for this project
            var existingProposal = await _proposalRepository.GetAllAsync(
                p => p.ProjectId == createDto.ProjectId && p.FreelancerId == freelancerId,
                includeDeleted: false
            );
            if (existingProposal.Any())
            {
                return null; // Proposal already exists for this project by this freelancer
            }

            // --- CONVERSION FROM IST STRING TO UTC DATETIME (DONE HERE) ---
            DateTime deadlineDateTimeUtc = ConvertIstStringToUtcDateTime(createDto.ProposedDeadline, nameof(createDto.ProposedDeadline));

            if (deadlineDateTimeUtc <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("deadline must be in the future.");
            }

            var proposal = _mapper.Map<Proposal>(createDto);
            proposal.FreelancerId = freelancerId;
            proposal.ProposedDeadLine = deadlineDateTimeUtc;
            proposal.Status = ProposalStatus.Pending; // Default status

            await _proposalRepository.AddAsync(proposal);
            await _proposalRepository.SaveChangesAsync();

            var createdProposal = await _proposalRepository.GetProposalDetailsAsync(proposal.Id);
            return _mapper.Map<ProposalResponseDto>(createdProposal);
        }

        public async Task<ProposalResponseDto?> GetProposalByIdAsync(Guid id)
        {
            var proposal = await _proposalRepository.GetProposalDetailsAsync(id);
            return proposal == null ? null : _mapper.Map<ProposalResponseDto>(proposal);
        }

        public async Task<ProposalResponseDto?> UpdateProposalStatusAsync(Guid proposalId, UpdateProposalStatusDto updateDto)
        {
            var proposal = await _proposalRepository.GetByIdAsync(proposalId);
            if (proposal == null)
            {
                return null;
            }


            // - Client can change Pending to Accepted/Rejected
            // - Freelancer can change Pending to Withdrawn
            // - Cannot change if already Accepted, Rejected, or Withdrawn
            if (proposal.Status == ProposalStatus.Accepted ||
                proposal.Status == ProposalStatus.Rejected ||
                proposal.Status == ProposalStatus.Withdrawn)
            {
                return null; // Cannot change status of an already finalized proposal
            }

            if (!Enum.TryParse<ProposalStatus>(updateDto.NewStatus, out var newStatus))
            {
                return null;
            }

            proposal.Status = newStatus;

            await _proposalRepository.UpdateAsync(proposal);
            await _proposalRepository.SaveChangesAsync();

            return _mapper.Map<ProposalResponseDto>(proposal);
        }

        public async Task<bool> DeleteProposalAsync(Guid id)
        {
            var proposal = await _proposalRepository.GetByIdAsync(id);
            if (proposal == null || proposal.Status != ProposalStatus.Pending) // Only allow deleting pending proposals
            {
                return false;
            }


            await _proposalRepository.DeleteAsync(id); // Soft delete
            await _proposalRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProposalResponseDto>> GetProposalsForProjectAsync(Guid projectId)
        {
            var proposals = await _proposalRepository.GetProposalsForProjectAsync(projectId, includeDetails: true);
            return _mapper.Map<IEnumerable<ProposalResponseDto>>(proposals);
        }

        public async Task<IEnumerable<ProposalResponseDto>> GetProposalsByFreelancerAsync(Guid freelancerId)
        {
            var proposals = await _proposalRepository.GetProposalsByFreelancerAsync(freelancerId, includeDetails: true);
            return _mapper.Map<IEnumerable<ProposalResponseDto>>(proposals);
        }
        
        public async Task<FileResponseDto?> UploadProposalAttachmentAsync(Guid proposalId, Guid uploaderId, IFormFile file)
        {
            var proposal = await _proposalRepository.GetByIdAsync(proposalId);
            if (proposal == null)
            {
                return null; // Proposal not found
            }

            // Calls FileService to handle the upload and database entry for the file,
            // associating it with the proposal via ProposalId.
            var uploadedFileDto = await _fileService.UploadFileAsync(file, uploaderId, FileCategory.ProposalAttachment, associatedEntityId: proposalId);

            return uploadedFileDto;
        }

        public async Task<bool> RemoveProposalAttachmentAsync(Guid proposalId, Guid attachmentId)
        {
            var proposal = await _proposalRepository.GetProposalDetailsAsync(proposalId); // Load with attachments
            if (proposal == null)
            {
                return false; // Proposal not found
            }

            // Verifies the attachment belongs to this proposal
            var attachmentToRemove = proposal.Attachments?.FirstOrDefault(a => a.Id == attachmentId);
            if (attachmentToRemove == null)
            {
                return false; // Attachment not found or not associated with this proposal
            }

            var success = await _fileService.DeleteFileAsync(attachmentId);

            return success;
        }

        public async Task<IEnumerable<FileResponseDto>> GetProposalAttachmentsMetadataAsync(Guid proposalId)
        {
            var proposal = await _proposalRepository.GetProposalDetailsAsync(proposalId); // Load proposal with Attachments
            if (proposal == null || proposal.Attachments == null)
            {
                return Enumerable.Empty<FileResponseDto>();
            }

            // Map the collection of Files to FileResponseDto
            return _mapper.Map<IEnumerable<FileResponseDto>>(proposal.Attachments.Where(f => !f.IsDeleted));
        }

        public async Task<FileResponseDto?> GetProposalAttachmentMetadataAsync(Guid attachmentId)
        {
            // Uses FileService to get the metadata of a specific file
            return await _fileService.GetFileMetadataAsync(attachmentId);
        }
    }
}
