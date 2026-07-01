using AutoMapper;
using LaTiendaApi.DTOs;
using LaTiendaApi.Models;

namespace LaTiendaApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Producto, ProductoDto>()
                .ForMember(dest => dest.IdCategoria,
                    opt => opt.MapFrom(src => src.IdCategoria))
                .ForMember(dest => dest.CategoriaNombre,
                    opt => opt.MapFrom(src => src.objCategoria != null
                        ? src.objCategoria.Nombre
                        : string.Empty));

            CreateMap<ProductoCreateDto, Producto>();

            CreateMap<Categoria, CategoriaDto>();
        }
    }
}