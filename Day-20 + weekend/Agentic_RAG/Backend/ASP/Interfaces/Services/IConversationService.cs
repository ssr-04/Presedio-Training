public interface IConversationService
{
    Task<NewConversationResponseDto?> CreateNewConversationAsync(Guid userId, QueryRequestDto request);
    Task<ContinueConversationResponseDto?> ContinueConversationAsync(Guid userId, Guid conversationId, QueryRequestDto request);
    Task<IEnumerable<ConversationSummaryDto>> GetAllConversationsForUserAsync(Guid userId);
    Task<ConversationDetailsDto?> GetConversationByIdAsync(Guid userId, Guid conversationId);
    Task<bool> DeleteConversationAsync(Guid userId, Guid conversationId);
}