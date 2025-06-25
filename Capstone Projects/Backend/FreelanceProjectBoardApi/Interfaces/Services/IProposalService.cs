using FreelanceProjectBoardApi.DTOs.Files;
using FreelanceProjectBoardApi.DTOs.Proposals;
using System.Collections.Generic;

namespace FreelanceProjectBoardApi.Services.Interfaces
{
    public interface IProposalService
    {
        Task<ProposalResponseDto?> CreateProposalAsync(Guid freelancerId, CreateProposalDto createDto);
        Task<ProposalResponseDto?> GetProposalByIdAsync(Guid id);
        Task<ProposalResponseDto?> UpdateProposalAsync(Guid id, UpdateProposalDto updateDto);
        Task<ProposalResponseDto?> UpdateProposalStatusAsync(Guid proposalId, UpdateProposalStatusDto updateDto); // For client to accept/reject, freelancer to withdraw
        Task<bool> DeleteProposalAsync(Guid id);
        Task<IEnumerable<ProposalResponseDto>> GetProposalsForProjectAsync(Guid projectId);
        Task<IEnumerable<ProposalResponseDto>> GetProposalsByFreelancerAsync(Guid freelancerId);

        Task<FileResponseDto?> UploadProposalAttachmentAsync(Guid proposalId, Guid uploaderId, IFormFile file);
        Task<bool> RemoveProposalAttachmentAsync(Guid proposalId, Guid attachmentId);
        Task<IEnumerable<FileResponseDto>> GetProposalAttachmentsMetadataAsync(Guid proposalId); // List of all attachments for a proposal
        Task<FileResponseDto?> GetProposalAttachmentMetadataAsync(Guid attachmentId); // Get single attachment metadata
    }
}
