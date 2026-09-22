using AutoMapper;
using certifyab.Core.Dto;
using certifyab.Data.Entities;

namespace certifyab.Mapper
{
    public class CertificateProfile : Profile
    {
        public CertificateProfile()
        {
            CreateMap<Certificate, CertificateDTO>();
            CreateMap<Certificate, CertificateCreateDTO>().ReverseMap();
            CreateMap<Certificate, CertificateCreatedDTO>();
            CreateMap<Certificate, CertificatePublicDTO>();
        }
    }
}