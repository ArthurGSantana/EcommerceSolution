using AutoMapper;

namespace HubMinified.Data.Mongo.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            var cInfo = System.Globalization.CultureInfo.InvariantCulture;
            
            CreateMap<Domain.MongoModels.Product, Entity.Product>().ReverseMap();
        }
    }
}