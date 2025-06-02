using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // For getting user ID from JWT

[ApiController]
    [Route("api/[controller]")] // Defines the base route, e.g., /api/conversations
    [Authorize] // All endpoints in this controller require authentication
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly ILogger<ConversationController> _logger; // Add logging

        public ConversationController(IConversationService conversationService, ILogger<ConversationController> logger)
        {
            _conversationService = conversationService;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            // Helper method to extract UserId from the authenticated user's claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogError("User ID claim not found or invalid in JWT for authenticated request.");
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }

        /// <summary>
        /// Starts a new conversation with an initial query.
        /// </summary>
        /// <param name="request">The initial query for the new conversation.</param>
        /// <returns>The RAG response and the new conversation ID.</returns>
        [HttpPost("new")] // Route: /api/conversations/new
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NewConversationResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StartNewConversation([FromBody] QueryRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid request for new conversation. Errors: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserId();
                _logger.LogInformation("User {UserId} starting new conversation with query: {Query}", userId, request.Query);
                var response = await _conversationService.CreateNewConversationAsync(userId, request);

                if (response == null)
                {
                    _logger.LogError("CreateNewConversationAsync returned null for user {UserId}", userId);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to create new conversation." });
                }

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access attempt: {Message}", ex.Message);
                return Unauthorized(new { error = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Application error starting new conversation for user {UserId}: {Message}", GetUserId(), ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred starting new conversation for user {UserId}", GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Continues an existing conversation with a new query.
        /// </summary>
        /// <param name="conversationId">The ID of the conversation to continue.</param>
        /// <param name="request">The new query for the conversation.</param>
        /// <returns>The RAG response for the new query.</returns>
        [HttpPost("{conversationId}/query")] // Route: /api/conversations/{conversationId}/query
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContinueConversationResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // If conversation not found or not owned
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ContinueConversation([FromRoute] Guid conversationId, [FromBody] QueryRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid request to continue conversation {ConversationId}. Errors: {Errors}",
                    conversationId, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserId();
                _logger.LogInformation("User {UserId} continuing conversation {ConversationId} with query: {Query}", userId, conversationId, request.Query);
                var response = await _conversationService.ContinueConversationAsync(userId, conversationId, request);

                if (response == null)
                {
                    _logger.LogWarning("Conversation {ConversationId} not found or not owned by user {UserId}", conversationId, userId);
                    return NotFound(new { error = "Conversation not found or access denied." }); // 404 Not Found
                }

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access attempt: {Message}", ex.Message);
                return Unauthorized(new { error = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Application error continuing conversation {ConversationId} for user {UserId}: {Message}", conversationId, GetUserId(), ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred continuing conversation {ConversationId} for user {UserId}", conversationId, GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Retrieves all conversations for the authenticated user.
        /// </summary>
        /// <returns>A list of conversation summaries.</returns>
        [HttpGet] // Route: /api/conversations
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConversationSummaryDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllConversationsForUser()
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Fetching all conversations for user: {UserId}", userId);
                var conversations = await _conversationService.GetAllConversationsForUserAsync(userId);
                return Ok(conversations);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access attempt: {Message}", ex.Message);
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred fetching all conversations for user {UserId}", GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Retrieves details of a specific conversation, including the last 5 exchanges.
        /// </summary>
        /// <param name="conversationId">The ID of the conversation to retrieve.</param>
        /// <returns>Conversation details with messages.</returns>
        [HttpGet("{conversationId}")] // Route: /api/conversations/{conversationId}
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ConversationDetailsDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetConversationById([FromRoute] Guid conversationId)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Fetching conversation {ConversationId} for user {UserId}", conversationId, userId);
                var conversationDetails = await _conversationService.GetConversationByIdAsync(userId, conversationId);

                if (conversationDetails == null)
                {
                    _logger.LogWarning("Conversation {ConversationId} not found or not owned by user {UserId}", conversationId, userId);
                    return NotFound(new { error = "Conversation not found or access denied." });
                }

                return Ok(conversationDetails);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access attempt: {Message}", ex.Message);
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred fetching conversation {ConversationId} for user {UserId}", conversationId, GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Deletes a specific conversation.
        /// </summary>
        /// <param name="conversationId">The ID of the conversation to delete.</param>
        /// <returns>No content on successful deletion.</returns>
        [HttpDelete("{conversationId}")] // Route: /api/conversations/{conversationId}
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteConversation([FromRoute] Guid conversationId)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("User {UserId} attempting to delete conversation {ConversationId}", userId, conversationId);
                var deleted = await _conversationService.DeleteConversationAsync(userId, conversationId);

                if (!deleted)
                {
                    _logger.LogWarning("Failed to delete conversation {ConversationId} or not found/owned by user {UserId}", conversationId, userId);
                    return NotFound(new { error = "Conversation not found or access denied." });
                }

                _logger.LogInformation("Conversation {ConversationId} successfully deleted by user {UserId}", conversationId, userId);
                return NoContent(); // 204 No Content
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access attempt: {Message}", ex.Message);
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred deleting conversation {ConversationId} for user {UserId}", conversationId, GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
            }
        }
    }