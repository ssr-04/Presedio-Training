using AutoMapper;
using System.Text.Json; // For JSON serialization/deserialization

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IFlaskApiService _flaskApiService;
    private readonly IMapper _mapper;
    private readonly ILogger<ConversationService> _logger; // Add logging

    public ConversationService(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IFlaskApiService flaskApiService,
        IMapper mapper,
        ILogger<ConversationService> logger)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _flaskApiService = flaskApiService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<NewConversationResponseDto?> CreateNewConversationAsync(Guid userId, QueryRequestDto request)
    {
        // 1. Create a new Conversation record
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Query.Length > 100 ? request.Query.Substring(0, 100) + "..." : request.Query, // Auto-title
            CreatedAt = DateTimeOffset.UtcNow,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };
        await _conversationRepository.AddAsync(conversation);
        await _conversationRepository.SaveChangesAsync(); // Save to get conversation ID

        // 2. Store user's initial query as a message
        var userMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = "user",
            Content = request.Query,
            CreatedAt = DateTimeOffset.UtcNow,
            FullResponseJson = null // User messages don't have Flask response JSON
        };
        await _messageRepository.AddAsync(userMessage);

        // 3. Call Flask RAG API
        var flaskRequest = new FlaskQueryRequestDto
        {
            Query = request.Query,
            ConversationId = conversation.Id.ToString(), // Pass new conversation ID to Flask
            ConversationHistory = new List<FlaskMessageDto>() // No history yet for new conv
        };
        var flaskResponse = await _flaskApiService.QueryRAGAgentAsync(flaskRequest);

        if (flaskResponse == null)
        {
            _logger.LogError("Flask API returned null response for new conversation. Query: {Query}", request.Query);
            // Consider rolling back transaction or marking conversation as failed
            throw new ApplicationException("RAG service did not return an answer.");
        }

        // 4. Store agent's response as a message
        var agentMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = "agent",
            Content = flaskResponse.Answer,
            CreatedAt = DateTimeOffset.UtcNow,
            FullResponseJson = JsonSerializer.Serialize(flaskResponse) // Store full JSON
        };
        await _messageRepository.AddAsync(agentMessage);

        // 5. Save all messages to DB
        if (!await _messageRepository.SaveChangesAsync())
        {
                _logger.LogError("Failed to save messages for new conversation. Query: {Query}", request.Query);
                throw new ApplicationException("Failed to save conversation messages.");
        }

        // 6. Map Flask response to our DTO for frontend
        var responseDto = _mapper.Map<NewConversationResponseDto>(flaskResponse);
        responseDto.ConversationId = conversation.Id;
        responseDto.MessageId = agentMessage.Id; // ID of the agent's message

        return responseDto;
    }


    public async Task<ContinueConversationResponseDto?> ContinueConversationAsync(Guid userId, Guid conversationId, QueryRequestDto request)
    {
        // 1. Validate conversation ownership
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null || conversation.UserId != userId)
        {
            _logger.LogWarning("Unauthorized attempt to access conversation {ConversationId} by user {UserId}", conversationId, userId);
            return null; // Or throw an UnauthorizedAccessException
        }

        // 2. Retrieve conversation history
        var rawMessages = await _messageRepository.GetAllByConversationIdAsync(conversationId);
        
        // Prepare history for Flask API (last 10 messages, 5 exchanges)
        // Filter only user and agent messages relevant for Flask context
        var flaskHistory = rawMessages
                            .Where(m => m.Role == "user" || m.Role == "agent") // Ensure only relevant roles
                            .OrderBy(m => m.CreatedAt) // Order chronologically
                            .TakeLast(10) // Get last 10 messages (5 user + 5 agent if pairs)
                            .Select(m => _mapper.Map<FlaskMessageDto>(m))
                            .ToList();

        // 3. Store user's new query
        var userMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = "user",
            Content = request.Query,
            CreatedAt = DateTimeOffset.UtcNow,
            FullResponseJson = null
        };
        await _messageRepository.AddAsync(userMessage);

        // 4. Call Flask RAG API
        var flaskRequest = new FlaskQueryRequestDto
        {
            Query = request.Query,
            ConversationId = conversation.Id.ToString(),
            ConversationHistory = flaskHistory
        };
        var flaskResponse = await _flaskApiService.QueryRAGAgentAsync(flaskRequest);

        if (flaskResponse == null)
        {
            _logger.LogError("Flask API returned null response for continuing conversation {ConversationId}. Query: {Query}", conversationId, request.Query);
            throw new ApplicationException("RAG service did not return an answer.");
        }

        // 5. Store agent's response
        var agentMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = "agent",
            Content = flaskResponse.Answer,
            CreatedAt = DateTimeOffset.UtcNow,
            FullResponseJson = JsonSerializer.Serialize(flaskResponse)
        };
        await _messageRepository.AddAsync(agentMessage);

        // 6. Update Conversation's LastUpdatedAt
        conversation.LastUpdatedAt = DateTimeOffset.UtcNow;
        await _conversationRepository.UpdateAsync(conversation);

        // 7. Save all messages and conversation update to DB
        // We can use a Transaction for atomicity if needed, but SaveChangesAsync is usually sufficient
        if (!await _messageRepository.SaveChangesAsync() || !await _conversationRepository.SaveChangesAsync())
        {
                _logger.LogError("Failed to save messages or update conversation {ConversationId}.", conversationId);
                throw new ApplicationException("Failed to save conversation messages or update conversation.");
        }

        // 8. Map Flask response to our DTO for frontend
        var responseDto = _mapper.Map<ContinueConversationResponseDto>(flaskResponse);
        responseDto.ConversationId = conversation.Id;
        responseDto.MessageId = agentMessage.Id;

        return responseDto;
    }

    public async Task<IEnumerable<ConversationSummaryDto>> GetAllConversationsForUserAsync(Guid userId)
    {
        var conversations = await _conversationRepository.GetAllByUserIdAsync(userId);
        var summaryDtos = _mapper.Map<IEnumerable<ConversationSummaryDto>>(conversations).ToList();

        // Optionally, populate LastMessagePreview for each summary
        foreach (var summaryDto in summaryDtos)
        {
            var lastMessage = (await _messageRepository.GetAllByConversationIdAsync(summaryDto.Id))
                                .OrderByDescending(m => m.CreatedAt)
                                .FirstOrDefault(m => m.Role == "user"); // Get last user query for preview
            summaryDto.LastMessagePreview = lastMessage?.Content ?? "No messages yet.";
        }

        return summaryDtos;
    }

    public async Task<ConversationDetailsDto?> GetConversationByIdAsync(Guid userId, Guid conversationId)
    {
        // 1. Validate conversation ownership
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null || conversation.UserId != userId)
        {
            _logger.LogWarning("Unauthorized attempt to retrieve conversation {ConversationId} by user {UserId}", conversationId, userId);
            return null; // Or throw UnauthorizedAccessException
        }

        // 2. Retrieve messages and filter for last 5 exchanges
        var allMessages = await _messageRepository.GetAllByConversationIdAsync(conversationId);

        // Filter for last 5 exchanges (i.e., last 10 messages)
        var lastFiveExchanges = allMessages
                                .OrderByDescending(m => m.CreatedAt) // Order from newest to oldest
                                .Take(10) // Take up to 10 messages
                                .OrderBy(m => m.CreatedAt) // Re-order to oldest to newest for chronological display
                                .ToList();
        
        // 3. Map to ConversationDetailsDto
        var conversationDetailsDto = _mapper.Map<ConversationDetailsDto>(conversation);
        conversationDetailsDto.Messages = _mapper.Map<List<MessageResponseDto>>(lastFiveExchanges);

        return conversationDetailsDto;
    }

    public async Task<bool> DeleteConversationAsync(Guid userId, Guid conversationId)
    {
        // 1. Validate conversation ownership
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null || conversation.UserId != userId)
        {
            _logger.LogWarning("Unauthorized attempt to delete conversation {ConversationId} by user {UserId}", conversationId, userId);
            return false;
        }

        // 2. Delete conversation (messages will cascade delete due to EF Core configuration)
        await _conversationRepository.DeleteAsync(conversation);
        return await _conversationRepository.SaveChangesAsync();
    }
}