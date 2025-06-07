using ApiCraftSystem.Data;
using ApiCraftSystem.Model;
using ApiCraftSystem.Repositories.AccountService.Dtos;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using ApiCraftSystem.Repositories.RateService.Dtos;
using ApiCraftSystem.Repositories.TenantService.Dto;
using AutoMapper;

namespace ApiCraftSystem.Helper.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApiStore, ApiStoreDto>().ReverseMap();
            CreateMap<ApiHeader, ApiHeaderDto>().ReverseMap();
            CreateMap<ApiMap, ApiMapDto>().ReverseMap();
            CreateMap<ApiStore, ApiStoreListDto>().ReverseMap();
            CreateMap<Rate, RateDto>().ReverseMap();
            CreateMap<Tenant, TenantDto>().ReverseMap();
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : null))
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant != null ? src.Tenant.Name : null))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))

                .ReverseMap();

            CreateMap<ApplicationUser, UpdateUserDto>()
                  .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                .ReverseMap();




        }
    }
}
