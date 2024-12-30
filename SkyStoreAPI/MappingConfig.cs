using AutoMapper;
using SkyStoreAPI.Models;
using SkyStoreAPI.Models.DTO;

namespace SkyStoreAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Category, CategoryCreateDTO>().ReverseMap();
            CreateMap<Category, CategoryUpdateDTO>().ReverseMap();

            CreateMap<Product, ProductDTO>().ReverseMap();
            CreateMap<Product, ProductCreateDTO>().ReverseMap();
            CreateMap<Product, ProductUpdateDTO>().ReverseMap();
        }
    }
}
