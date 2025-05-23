using ApiCraftSystem.Model;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
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


        }
    }
}
