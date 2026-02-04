using AutoMapper;
using KRT.Onboarding.Application.Accounts.DTOs.Responses;
using KRT.Onboarding.Domain.Entities;

namespace KRT.Onboarding.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Account, AccountResponse>()
            .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.AccountNumber))
            // Os outros campos (CustomerDocument, etc) agora existem na entidade como propriedades computadas
            // ou têm o mesmo nome, então o AutoMapper resolve sozinho.
            ;
    }
}
