
using AutoMapper;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        var istOffset = new TimeSpan(5, 30, 0);

        // Customer Mappings
        CreateMap<CreateCustomerDto, Customer>()
                //  DateOfBirth is converted to UTC before mapping to entity
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth.ToUniversalTime()));
        CreateMap<UpdateCustomerDto, Customer>()
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom((src, dest) => src.DateOfBirth.HasValue ? src.DateOfBirth.Value.ToUniversalTime() : dest.DateOfBirth))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Customer, CustomerResponseDto>()
            .ForMember(dest => dest.Accounts, opt => opt.MapFrom(src => src.Accounts))
             .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth.ToOffset(istOffset)))
             .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.RegistrationDate.ToOffset(istOffset))); // Mapping the collection of accounts


        // Account Mappings
        CreateMap<CreateAccountDto, Account>()
            .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => Enum.Parse<AccountType>(src.AccountType))); // Mapping string to enum
        CreateMap<Account, AccountResponseDto>()
            .ForMember(dest => dest.CustomerFirstName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName : string.Empty))
            .ForMember(dest => dest.CustomerLastName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.LastName : string.Empty))
            .ForMember(dest => dest.OpeningDate, opt => opt.MapFrom(src => src.OpeningDate.ToOffset(istOffset)));

        // Transaction Mappings
        CreateMap<Transaction, TransactionResponseDto>()
                .ForMember(dest => dest.AccountNumber, opt => opt.MapFrom(src => src.Account != null ? src.Account.AccountNumber : string.Empty))
                .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => src.TransactionDate.ToOffset(istOffset)));;
    }
}