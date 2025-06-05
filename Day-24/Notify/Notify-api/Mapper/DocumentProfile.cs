using AutoMapper;
using NotifyAPI.DTOs;

public class DocumentProfile : Profile
{
    public DocumentProfile()
    {
        CreateMap<FileData, DocumentMetadataDto>()
            .ForSourceMember(src => src.Content, opt => opt.DoNotValidate());

    
    }
}