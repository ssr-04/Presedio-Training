using System.Net.Http.Json; // For ReadFromJsonAsync, PostAsJsonAsync
public class FlaskApiService : IFlaskApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FlaskApiService> _logger; // Add logging

    public FlaskApiService(HttpClient httpClient, ILogger<FlaskApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        // BaseAddress for the Flask API should be configured via HttpClient factory in Program.cs
        // e.g., httpClient.BaseAddress = new Uri("http://localhost:8000");
    }

    public async Task<FlaskRAGResponseDto?> QueryRAGAgentAsync(FlaskQueryRequestDto request)
    {
        try
        {
            _logger.LogInformation("Calling Flask RAG API for query: {Query}", request.Query);
            var response = await _httpClient.PostAsJsonAsync("/api/agentic_qna", request);

            response.EnsureSuccessStatusCode(); // Throws an exception for 4xx or 5xx responses

            var flaskResponse = await response.Content.ReadFromJsonAsync<FlaskRAGResponseDto>();

            _logger.LogInformation("Successfully received response from Flask API. Path: {Path}, Cached: {Cached}",
                flaskResponse?.AgentPathTaken, flaskResponse?.Cached);

            return flaskResponse;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP request to Flask API failed. Status: {StatusCode}. Message: {Message}",
                httpEx.StatusCode, httpEx.Message);
            // Optionally read response content for more details on HTTP error
            string? errorContent = null;
            if (httpEx.StatusCode != null)
            {
                try { errorContent = httpEx.Data["responseContent"]?.ToString(); } catch { }
                _logger.LogError("Flask API HTTP error response content: {Content}", errorContent);
            }

            throw new ApplicationException($"Error communicating with RAG service: {httpEx.Message}", httpEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while calling Flask RAG API.");
            throw new ApplicationException("An unexpected error occurred while processing your request.", ex);
        }
    }
}