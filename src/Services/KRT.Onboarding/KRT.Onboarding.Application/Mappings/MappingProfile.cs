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
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CustomerName))
            .ForMember(dest => dest.CustomerDocument, opt => opt.MapFrom(src => src.Cpf))
            .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active")); // Mock status
    }
}
