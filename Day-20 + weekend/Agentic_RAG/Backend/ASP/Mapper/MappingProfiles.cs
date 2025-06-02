using AutoMapper;
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // --- Authentication Mappings ---
        CreateMap<User, AuthResponseDto>()
            .ForMember(dest => dest.Token, opt => opt.Ignore()); // Token is set separately by AuthService

        // --- Flask API Request/Response Mappings ---
        // From our Message model (for sending history to Flask)
        CreateMap<Message, FlaskMessageDto>()
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role));

        // From our QueryRequestDto (for sending a new query to Flask)
        CreateMap<QueryRequestDto, FlaskQueryRequestDto>()
            .ForMember(dest => dest.Query, opt => opt.MapFrom(src => src.Query))
            .ForMember(dest => dest.ConversationId, opt => opt.Ignore()) // Set dynamically in service
            .ForMember(dest => dest.ConversationHistory, opt => opt.Ignore()); // Set dynamically in service

        // From FlaskSourceDto to our internal SourceResponseDto
        CreateMap<FlaskSourceDto, SourceResponseDto>();

        // From FlaskRAGResponseDto to our internal RAGResponseDto (used for storing and returning)
        CreateMap<FlaskRAGResponseDto, RAGResponseDto>()
            .ForMember(dest => dest.Sources, opt => opt.MapFrom(src => src.Sources))
            .ForMember(dest => dest.ConversationId, opt => opt.Ignore()) // Set dynamically in service
            .ForMember(dest => dest.MessageId, opt => opt.Ignore());     // Set dynamically in service

        // --- Conversation & Message Mappings for .NET API Responses ---
        CreateMap<Conversation, ConversationSummaryDto>()
            .ForMember(dest => dest.LastMessagePreview, opt => opt.Ignore()); // Will be populated in service if needed

        CreateMap<Message, MessageResponseDto>(); // Simple 1:1 mapping

        // Mapping for NewConversationResponseDto (inherits from RAGResponseDto)
        CreateMap<FlaskRAGResponseDto, NewConversationResponseDto>()
            .IncludeBase<FlaskRAGResponseDto, RAGResponseDto>() // Inherit base mappings
            .ForMember(dest => dest.ConversationId, opt => opt.Ignore()) // Set dynamically in service
            .ForMember(dest => dest.MessageId, opt => opt.Ignore()); // Set dynamically in service

        // Mapping for ContinueConversationResponseDto (inherits from RAGResponseDto)
        CreateMap<FlaskRAGResponseDto, ContinueConversationResponseDto>()
            .IncludeBase<FlaskRAGResponseDto, RAGResponseDto>() // Inherit base mappings
            .ForMember(dest => dest.ConversationId, opt => opt.Ignore()) // Set dynamically in service
            .ForMember(dest => dest.MessageId, opt => opt.Ignore()); // Set dynamically in service


        // When mapping ConversationDetailsDto, you'll typically load messages separately and map them
        CreateMap<Conversation, ConversationDetailsDto>()
            .ForMember(dest => dest.Messages, opt => opt.Ignore()); // Messages will be mapped/added in service
    }
}