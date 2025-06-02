public interface IFlaskApiService
{
    Task<FlaskRAGResponseDto?> QueryRAGAgentAsync(FlaskQueryRequestDto request);
}
