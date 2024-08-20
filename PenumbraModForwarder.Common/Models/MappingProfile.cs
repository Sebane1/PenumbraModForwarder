using AutoMapper;

namespace PenumbraModForwarder.Common.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<OldConfigurationModel, ConfigurationModel>()
            .ForMember(dest => dest.DownloadPath, opt => opt.MapFrom(src => src.DownloadPath))
            .ForMember(dest => dest.AutoLoad, opt => opt.MapFrom(src => src.AutoLoad))
            .ForMember(dest => dest.AutoDelete, opt => opt.MapFrom(src => src.AutoDelete))
            .ForMember(dest => dest.TexToolPath, opt => opt.MapFrom(src => src.TexToolPath));
    }
}