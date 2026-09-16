using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using certifyAb.Core.Dto;
using certifyAb.Data.Entities;

namespace certifyAb.Mapper
{
    public class CertificateProfile : Profile
    {
        public CertificateProfile()
        {
            CreateMap<Certificate, CertificateDTO>();
            CreateMap<Certificate, CertificateCreateDTO>().ReverseMap();
            CreateMap<Certificate, CertificateCreatedDTO>();
        }
    }
}